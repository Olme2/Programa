using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Html;
using entornoPolleria;
using VentasVM;

public class VentaRepository : IVentaRepository
{
    private readonly AppDbContext _context;
    public VentaRepository(AppDbContext context)
    {
        _context = context;
    }
    public IEnumerable<ListarVentasVM> ObtenerListadoVentas(IndexVentasVM filtro)
    {
        var fechaInicio = DateOnly.FromDateTime(filtro.FechaInicio);
        var fechaFin = DateOnly.FromDateTime(filtro.FechaFin);
        var horaInicio = new TimeOnly();
        var horaFin = new TimeOnly();
        switch (filtro.Turno)
        {
            case IndexVentasVM.Turnos.Mañana:
                horaInicio = new TimeOnly(9,0);
                horaFin = new TimeOnly(14,0);
                break;
            case IndexVentasVM.Turnos.Tarde:
                horaInicio = new TimeOnly(17,30);
                horaFin = new TimeOnly(21,30);
                break;
            default:
                horaInicio = new TimeOnly(0,0);
                horaFin = new TimeOnly(23,59);
                break;
        }

        var query = _context.Ventas.AsQueryable();

        // Filtrado
        query = query.Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin && v.Hora >= horaInicio && v.Hora <= horaFin);

        if (filtro.IdMetodoPago.HasValue)
        {
            query = query.Where(v => v.IdMetodo == filtro.IdMetodoPago.Value);
        }

        if (!string.IsNullOrEmpty(filtro.Busqueda))
        {
            string busquedaLower = filtro.Busqueda.ToLower();
            query = query.Where(v =>
                v.DetallesVenta.Any(d => d.Producto.Producto.ToLower().Contains(busquedaLower)) ||
                v.VentaPromociones.Any(vp => vp.Promocion.Promocion.ToLower().Contains(busquedaLower))
            );
        }

        // Proyección al ViewModel
        var ventasVM = query
            .Include(v => v.Metodo)
            .Include(v => v.DetallesVenta).ThenInclude(dv => dv.Producto)
            .Include(v => v.VentaPromociones).ThenInclude(vp => vp.Promocion)
            .Select(v => new ListarVentasVM()
            {
                IdVenta = v.IdVenta,
                Metodo = v.Metodo.Metodo,
                ProductosYPromociones = v.VentaPromociones.Any()
                    ? string.Concat(
                            string.Join("<br>", v.VentaPromociones.Select(vp => vp.Promocion.Promocion)),
                            "<br>",
                            string.Join("<br>", v.DetallesVenta.Select(dv => dv.Producto.Producto)))
                    : string.Join("<br>", v.DetallesVenta.Select(dv => dv.Producto.Producto)),
                Total = v.CalcularPrecioTotal(),
                Fecha = v.Fecha,
                Hora = v.Hora,
                Detalle = v.Detalle,
                Redondeo = v.Redondeo
            });

        return ventasVM.OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Hora).ToList();
    }
    public Ventas? ObtenerVentaPorId(long id)
    {
        try
        {
            return _context.Ventas
                .Include(v => v.Metodo)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(dv => dv.Producto)
                .Include(v => v.VentaPromociones)
                    .ThenInclude(vp => vp.Promocion)
                        .ThenInclude(p => p.DetallesPromocion)
                            .ThenInclude(dp => dp.Producto)
                .FirstOrDefault(v => v.IdVenta == id);
        }
        catch (Exception)
        {
            throw;
        }
    }
    public void CrearVenta(Ventas nuevaVenta)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            // Paso 1: Validar stock antes de hacer nada.
            ValidarStockParaVenta(nuevaVenta);
            // Paso 2: Añadir la venta al contexto.
            // EF Core se encargará de añadir los detalles en cascada.
            _context.Ventas.Add(nuevaVenta);
            // Paso 3: Guardar cambios. Los triggers de la BD se ejecutarán aquí.
            _context.SaveChanges();
            // Paso 4: Si todo fue bien, confirmar la transacción.
            transaction.Commit();
        }
        catch (Exception)
        {
            // Si algo falla (ej. validación o trigger), revertir todo.
            transaction.Rollback();
            throw; // Relanzar la excepción para que el controlador la maneje.
        }
    }
    public void ActualizarVenta(Ventas ventaModificada)
    {
        // La lógica de actualización es compleja y requiere un manejo cuidadoso
        // de las entidades para evitar conflictos con el tracking de EF Core.
        // Por ahora, implementamos la estructura y la lógica se puede añadir después.
        _context.Ventas.Update(ventaModificada);
        _context.SaveChanges();
    }
    public void EliminarVenta(long id)
    {
        var venta = _context.Ventas.Find(id);
        if (venta != null)
        {
            // Gracias al borrado en cascada, al eliminar la venta,
            // EF Core y la BD eliminarán los DetallesVenta y VentaPromociones asociados.
            // Los triggers de la BD se encargarán de devolver el stock.
            _context.Ventas.Remove(venta);
            _context.SaveChanges();
        }
    }
    // --- Métodos Privados de Ayuda ---
    private void ValidarStockParaVenta(Ventas venta)
    {
        // Validar productos individuales
        foreach (var detalle in venta.DetallesVenta)
        {
            var producto = _context.Productos.Find(detalle.IdProducto);
            if (producto == null || producto.Stock < detalle.Cantidad)
            {
                throw new InvalidOperationException($"Stock insuficiente para el producto '{producto?.Producto ?? "ID: " + detalle.IdProducto}'. Stock disponible: {producto?.Stock ?? 0}.");
            }
        }
        // Validar productos dentro de promociones
        foreach (var ventaPromo in venta.VentaPromociones)
        {
            // Necesitamos cargar la promoción y sus detalles desde la BD
            var promocion = _context.Promociones
                .Include(p => p.DetallesPromocion)
                .AsNoTracking() // Usamos AsNoTracking para no interferir con el contexto principal
                .FirstOrDefault(p => p.IdPromocion == ventaPromo.IdPromocion);
            if (promocion == null) continue;
            foreach (var detallePromo in promocion.DetallesPromocion)
            {
                var producto = _context.Productos.Find(detallePromo.IdProducto);
                var cantidadRequerida = ventaPromo.Cantidad * detallePromo.Cantidad;
                if (producto == null || producto.Stock < cantidadRequerida)
                {
                    throw new InvalidOperationException($"Stock insuficiente para el producto '{producto?.Producto ?? "ID: " + detallePromo.IdProducto}' dentro de la promoción '{promocion.Promocion}'. Stock disponible: {producto?.Stock ?? 0}, requerido: {cantidadRequerida}.");
                }
            }
        }
    }
}