using Microsoft.EntityFrameworkCore;
using entornoPolleria;
using ComprasVM;

public class ComprasRepository : IComprasRepository
{
    private readonly AppDbContext _context;
    public ComprasRepository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<ListarComprasVM> ObtenerListadoCompras(IndexComprasVM filtro)
    {
        var fechaInicio = DateOnly.FromDateTime(filtro.FechaInicio);
        var fechaFin = DateOnly.FromDateTime(filtro.FechaFin);

        var query = _context.Compras.AsQueryable();

        query = query.Where(c => c.Fecha >= fechaInicio && c.Fecha <= fechaFin);

        if (filtro.IdProveedor.HasValue)
            query = query.Where(c => c.IdProveedor == filtro.IdProveedor.Value);

        return query
            .Include(c => c.Proveedor)
            .Select(c => new ListarComprasVM()
            {
                IdCompra = c.IdCompra,
                Proveedor = c.Proveedor.Proveedor,
                Productos = string.Join(", ", c.DetallesCompra.Select(dc => dc.Producto.Producto)),
                // Calcular total directamente en SQL sumando los detalles
                Total = c.DetallesCompra.Sum(dc => dc.Cantidad * dc.CostoUnitario),
                Fecha = c.Fecha,
                Detalle = c.Detalle,
            })
            .OrderByDescending(c => c.IdCompra)
            .ToList();
    }

    public Compras? ObtenerPorId(int id)
    {
        return _context.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.DetallesCompra)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .FirstOrDefault(c => c.IdCompra == id);
    }

    // Los triggers de Postgres (actualizar_stock_por_compra) gestionan el stock automáticamente.
    public void Crear(Compras compra)
    {
        try
        {
            compra.IdCompra = 0; // Forzar Identity Generation
            _context.Compras.Add(compra);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR (Crear): {ex.Message} | Inner: {ex.InnerException?.Message}");
            throw;
        }
    }

    // El trigger de Postgres gestiona el recalculo de stock al insertar/eliminar detalles.
    public void Actualizar(int idCompra, Compras compraActualizada)
    {
        try
        {
            var compraOriginal = _context.Compras
                .AsTracking()
                .FirstOrDefault(c => c.IdCompra == idCompra);

            if (compraOriginal == null)
                throw new InvalidOperationException($"No se encontró la compra con ID {idCompra}.");

            // 1. Actualizar cabecera
            compraOriginal.IdProveedor = compraActualizada.IdProveedor;
            compraOriginal.Fecha = compraActualizada.Fecha;
            compraOriginal.Detalle = compraActualizada.Detalle;
            _context.SaveChanges();

            // 2. Eliminar todos los detalles anteriores por SQL (trigger revierta el stock)
            _context.Database.ExecuteSqlRaw("DELETE FROM detalle_compra WHERE id_compra = {0}", idCompra);

            // 3. Insertar los nuevos detalles (trigger suma el nuevo stock)
            foreach (var d in compraActualizada.DetallesCompra)
            {
                _context.Database.ExecuteSqlRaw(
                    "INSERT INTO detalle_compra (id_compra, id_producto, cantidad, costo_unitario) VALUES ({0}, {1}, {2}, {3})",
                    idCompra, d.IdProducto, d.Cantidad, d.CostoUnitario);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR (Actualizar): {ex.Message} | Inner: {ex.InnerException?.Message}");
            throw;
        }
    }

    // El trigger de Postgres invierte el stock al eliminar cada detalle.
    public void Eliminar(int id)
    {
        try
        {
            // 1. Eliminar los detalles primero con SQL directo (no hay ON DELETE CASCADE en la FK)
            _context.Database.ExecuteSqlRaw("DELETE FROM detalle_compra WHERE id_compra = {0}", id);

            // 2. Ahora eliminar la cabecera
            var compra = _context.Compras.Find(id);
            if (compra == null) return;
            _context.Compras.Remove(compra);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR (Eliminar): {ex.Message} | Inner: {ex.InnerException?.Message}");
            throw;
        }
    }
}