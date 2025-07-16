using entornoPolleria;
using Microsoft.EntityFrameworkCore;

public class VentasRepository : IVentasRepository
{
    private readonly AppDbContext _context;
    public VentasRepository(AppDbContext context)
    {
        _context = context;
    }

    public void CrearVenta(Ventas venta, List<DetallesVentas> detalles)
    {
        if (!ValidarStock(detalles, out var mensajeError))
            throw new Exception($"Stock insuficiente: {mensajeError}");
        if (!ValidarMetodoDePago(venta.IdMetodo))
            throw new Exception("Método de pago inválido");
        // Calcular totales
        decimal total = 0, costo = 0;
        foreach (var d in detalles)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.IdProducto == d.IdProducto);
            if (producto == null) throw new Exception($"Producto no encontrado: {d.IdProducto}");
            total += d.Promocion ? d.PrecioPromo * d.Cantidad : producto.Precio * d.Cantidad;
            costo += d.Promocion ? d.CostoPromo * d.Cantidad : producto.Costo * d.Cantidad;
            // Descontar stock
            producto.Stock -= d.Cantidad;
        }
        venta.Total = total;
        venta.Costo = costo;
        venta.Ganancia = total - costo;
        venta.PorcentajeGanancia = costo > 0 ? ((total - costo) / costo) * 100 : 0;
        venta.Fecha = DateOnly.FromDateTime(DateTime.Now);
        venta.Hora = TimeOnly.FromDateTime(DateTime.Now);
        _context.Ventas.Add(venta);
        _context.SaveChanges();
        // Asignar idVenta a los detalles
        foreach (var d in detalles)
            d.IdVenta = venta.IdVenta;
        _context.DetallesVentas.AddRange(detalles);
        _context.SaveChanges();
    }

    public void ModificarVenta(Ventas venta, List<DetallesVentas> nuevosDetalles)
    {
        var ventaExistente = _context.Ventas.Include(v => v.Detalles).FirstOrDefault(v => v.IdVenta == venta.IdVenta);
        if (ventaExistente == null) throw new Exception("Venta no encontrada");
        // Revertir stock de los detalles anteriores
        foreach (var d in ventaExistente.Detalles ?? new List<DetallesVentas>())
        {
            var producto = _context.Productos.FirstOrDefault(p => p.IdProducto == d.IdProducto);
            if (producto != null) producto.Stock += d.Cantidad;
        }
        _context.DetallesVentas.RemoveRange(ventaExistente.Detalles ?? new List<DetallesVentas>());
        _context.SaveChanges();
        // Validar y descontar stock de los nuevos detalles
        if (!ValidarStock(nuevosDetalles, out var mensajeError))
            throw new Exception($"Stock insuficiente: {mensajeError}");
        decimal total = 0, costo = 0;
        foreach (var d in nuevosDetalles)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.IdProducto == d.IdProducto);
            if (producto == null) throw new Exception($"Producto no encontrado: {d.IdProducto}");
            total += d.Promocion ? d.PrecioPromo * d.Cantidad : producto.Precio * d.Cantidad;
            costo += d.Promocion ? d.CostoPromo * d.Cantidad : producto.Costo * d.Cantidad;
            producto.Stock -= d.Cantidad;
        }
        ventaExistente.IdMetodo = venta.IdMetodo;
        ventaExistente.Total = total;
        ventaExistente.Costo = costo;
        ventaExistente.Ganancia = total - costo;
        ventaExistente.PorcentajeGanancia = costo > 0 ? ((total - costo) / costo) * 100 : 0;
        ventaExistente.Detalle = venta.Detalle;
        _context.DetallesVentas.AddRange(nuevosDetalles);
        _context.SaveChanges();
    }

    public void EliminarVenta(long idVenta)
    {
        var venta = _context.Ventas.Include(v => v.Detalles).FirstOrDefault(v => v.IdVenta == idVenta);
        if (venta == null) throw new Exception("Venta no encontrada");
        // Revertir stock
        foreach (var d in venta.Detalles ?? new List<DetallesVentas>())
        {
            var producto = _context.Productos.FirstOrDefault(p => p.IdProducto == d.IdProducto);
            if (producto != null) producto.Stock += d.Cantidad;
        }
        _context.DetallesVentas.RemoveRange(venta.Detalles ?? new List<DetallesVentas>());
        _context.Ventas.Remove(venta);
        _context.SaveChanges();
    }

    public Ventas? ObtenerVentaPorId(long idVenta)
    {
        return _context.Ventas.Include(v => v.Detalles).ThenInclude(d => d.Producto).Include(v => v.MetodoDePago).FirstOrDefault(v => v.IdVenta == idVenta);
    }

    public List<Ventas> ListarVentas()
    {
        return _context.Ventas.Include(v => v.MetodoDePago).ToList();
    }

    public List<DetallesVentas> ObtenerDetallesPorVenta(long idVenta)
    {
        return _context.DetallesVentas.Include(d => d.Producto).Where(d => d.IdVenta == idVenta).ToList();
    }

    public bool ValidarStock(List<DetallesVentas> detalles, out string mensajeError)
    {
        mensajeError = "";
        foreach (var d in detalles)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.IdProducto == d.IdProducto);
            if (producto == null)
            {
                mensajeError = $"Producto no encontrado: {d.IdProducto}";
                return false;
            }
            if (producto.Stock < d.Cantidad)
            {
                mensajeError = $"Stock insuficiente para {producto.Producto} (ID: {producto.IdProducto})";
                return false;
            }
        }
        return true;
    }

    public bool ValidarMetodoDePago(short idMetodo)
    {
        return _context.MetodosDePago.Any(m => m.IdMetodo == idMetodo);
    }
} 