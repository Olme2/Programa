using VentasPromocionesVM;
using DetallesVentasVM;
using VentasVM;
public class Ventas
{
    public long IdVenta { get; set; }
    public short IdMetodo { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public string? Detalle { get; set; }
    public decimal Redondeo { get; set; }
    public string Tipo { get; set; } = "venta"; // venta | consumo | produccion
    public List<DetallesVentas> _detallesVenta = new List<DetallesVentas>();
    public IReadOnlyCollection<DetallesVentas> DetallesVenta => _detallesVenta.AsReadOnly();
    public List<VentasPromociones> _ventaPromociones = new List<VentasPromociones>();
    public IReadOnlyCollection<VentasPromociones> VentaPromociones => _ventaPromociones.AsReadOnly();
    public virtual MetodosPago Metodo { get; private set; }
    private Ventas()
    {
        Metodo = null!;
    }

    private Ventas(short idMetodo, DateOnly fecha, TimeOnly hora, string? detalle, decimal redondeo, List<DetallesVentas> detalles, List<VentasPromociones> promociones)
    {
        IdMetodo = idMetodo;
        Fecha = fecha;
        Hora = hora;
        Detalle = detalle;
        Redondeo = redondeo;
        _detallesVenta = detalles;
        _ventaPromociones = promociones;
        Metodo = null!;
    }

    public static Ventas CrearDesdeViewModel(AltaVentaVM ventaVM)
    {
        var detalles = ventaVM.DetallesVenta.Select(DetallesVentas.CrearDesdeViewModel).ToList();
        var promociones = ventaVM.VentaPromociones.Select(VentasPromociones.CrearDesdeViewModel).ToList();
        return new Ventas(ventaVM.IdMetodo, ventaVM.Fecha, ventaVM.Hora, ventaVM.Detalle, ventaVM.Redondeo, detalles, promociones);
    }

    // Crea un egreso interno (consumo o produccion) que usa la infra de venta sin precio de venta
    public static Ventas CrearEgreso(string tipo, short idMetodo, DateOnly fecha, TimeOnly hora, string? detalle, List<DetallesVentas> detalles)
    {
        var v = new Ventas(idMetodo, fecha, hora, detalle, 0, detalles, new List<VentasPromociones>());
        v.Tipo = tipo;
        return v;
    }
    public void ActualizarDesdeViewModel(ModificarVentaVM ventaVM)
    {
        IdMetodo = ventaVM.IdMetodo;
        Fecha = ventaVM.Fecha;
        Hora = ventaVM.Hora;
        Detalle = ventaVM.Detalle;
        Redondeo = ventaVM.Redondeo;
        var detallesActualizados = ventaVM.DetallesVenta.Select(DetallesVentas.CrearDesdeViewModel).ToList();
        var promocionesActualizadas = ventaVM.VentaPromociones.Select(VentasPromociones.CrearDesdeViewModel).ToList();
        LimpiarDetalles();
        foreach (var detalle in detallesActualizados)
        {
            AgregarDetalle(detalle);
        }
        foreach (var promocion in promocionesActualizadas)
        {
            AgregarPromocion(promocion);
        }
    }

    public void ActualizarDetalles(List<DetalleVentaVM> detallesVM, List<VentaPromocionVM> promocionesVM)
    {
        var idsProductosEnViewModel = detallesVM.Select(d => d.IdProducto).ToList();
        var detallesAEliminar = _detallesVenta.Where(d => !idsProductosEnViewModel.Contains(d.IdProducto)).ToList();
        foreach (var detalle in detallesAEliminar)
        {
            _detallesVenta.Remove(detalle);
        }

        foreach (var detalleVM in detallesVM)
        {
            var detalleExistente = _detallesVenta.FirstOrDefault(d => d.IdProducto == detalleVM.IdProducto);
            if (detalleExistente != null)
            {
                detalleExistente.Cantidad = detalleVM.Cantidad;
            }
            else
            {
                _detallesVenta.Add(DetallesVentas.CrearDesdeViewModel(detalleVM));
            }
        }

        var idsPromocionesEnViewModel = promocionesVM.Select(p => p.IdPromocion).ToList();
        var promocionesAEliminar = _ventaPromociones.Where(p => !idsPromocionesEnViewModel.Contains(p.IdPromocion)).ToList();
        foreach (var promocion in promocionesAEliminar)
        {
            _ventaPromociones.Remove(promocion);
        }

        foreach (var promocionVM in promocionesVM)
        {
            var promocionExistente = _ventaPromociones.FirstOrDefault(p => p.IdPromocion == promocionVM.IdPromocion);
            if (promocionExistente != null)
            {
                promocionExistente.Cantidad = promocionVM.Cantidad;
            }
            else
            {
                _ventaPromociones.Add(VentasPromociones.CrearDesdeViewModel(promocionVM));
            }
        }

    }


    public void LimpiarDetalles()
    {
        _detallesVenta.Clear();
        _ventaPromociones.Clear();
    }

    public void AgregarDetalle(DetallesVentas detalle)
    {
        _detallesVenta.Add(detalle);
    }

    public void AgregarPromocion(VentasPromociones promociones)
    {
        _ventaPromociones.Add(promociones);
    }

    public decimal CalcularCostoTotal()
    {
        return _detallesVenta.Sum(detalle => detalle.CalcularCosto()) + _ventaPromociones.Sum(promocion => promocion.CalcularCosto());
    }

    public decimal CalcularPrecioTotal()
    {
        return _detallesVenta.Sum(detalle => detalle.CalcularPrecio()) + _ventaPromociones.Sum(promocion => promocion.CalcularPrecio()) + Redondeo;
    }
}