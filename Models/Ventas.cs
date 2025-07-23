using VentasVM;

public class Ventas
{
    public long IdVenta { get; private set; }
    public short IdMetodo { get; private set; }
    public DateOnly Fecha { get; private set; }
    public TimeOnly Hora { get; private set; }
    public string? Detalle { get; private set; }
    public List<DetallesVentas> _detallesVenta = new List<DetallesVentas>();
    public IReadOnlyCollection<DetallesVentas> DetallesVenta => _detallesVenta.AsReadOnly();
    public List<VentasPromociones> _ventaPromociones = new List<VentasPromociones>();
    public IReadOnlyCollection<VentasPromociones> VentaPromociones => _ventaPromociones.AsReadOnly();
    public virtual MetodosPago Metodo { get; private set; }
    private Ventas()
    {
        Metodo = null!;
    }

    private Ventas(short idMetodo, DateOnly fecha, TimeOnly hora, string? detalle, List<DetallesVentas> detalles, List<VentasPromociones> promociones)
    {
        IdMetodo = idMetodo;
        Fecha = fecha;
        Hora = hora;
        Detalle = detalle;
        _detallesVenta = detalles;
        _ventaPromociones = promociones;
        Metodo = null!;
    }

    public static Ventas CrearDesdeViewModel(AltaVentaVM ventaVM)
    {
        var detalles = ventaVM.DetallesVenta.Select(DetallesVentas.CrearDesdeViewModel).ToList();
        var promociones = ventaVM.VentaPromociones.Select(VentasPromociones.CrearDesdeViewModel).ToList();
        return new Ventas(ventaVM.IdMetodo, ventaVM.Fecha, ventaVM.Hora, ventaVM.Detalle, detalles, promociones);
    }
    public void ActualizarDesdeViewModel(ModificarVentaVM ventaVM)
    {
        IdMetodo = ventaVM.IdMetodo;
        Fecha = ventaVM.Fecha;
        Hora = ventaVM.Hora;
        Detalle = ventaVM.Detalle;
        LimpiarDetalles();
        var detallesActualizados = ventaVM.DetallesVenta.Select(DetallesVentas.CrearDesdeViewModel).ToList();
        var promocionesActualizadas = ventaVM.VentaPromociones.Select(VentasPromociones.CrearDesdeViewModel).ToList();
        foreach (var detalle in detallesActualizados)
        {
            AgregarDetalle(detalle);
        }
        foreach (var promocion in promocionesActualizadas)
        {
            AgregarPromocion(promocion);
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
        return _detallesVenta.Sum(detalle => detalle.CalcularPrecio()) + _ventaPromociones.Sum(promocion => promocion.CalcularPrecio());
    }
}