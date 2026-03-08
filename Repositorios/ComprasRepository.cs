using Microsoft.EntityFrameworkCore;
using entornoPolleria; // Asegúrate que este es el namespace de tu DbContext
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

        query.Where(c => c.Fecha >= fechaInicio && c.Fecha <= fechaFin);

        if (filtro.IdProveedor.HasValue)
        {
            query = query.Where(c => c.IdProveedor == filtro.IdProveedor.Value);
        }

        var comprasVM = query
            .Include(c => c.Proveedor)
            .Select(c => new ListarComprasVM()
            {
                IdCompra = c.IdCompra,
                Proveedor = c.Proveedor.Proveedor,
                Productos = string.Join(",", c.DetallesCompra.Select(dc => dc.Producto.Producto)),
                Total = c.CalcularTotal(),
                Fecha = c.Fecha,
                Detalle = c.Detalle,
            });
        
        return comprasVM.OrderByDescending(c => c.Fecha).ToList();
    }

    public Compras? ObtenerPorId(int id)
    {
        try
        {
            return _context.Compras
                .Include(c => c.Proveedor) // Proveedor
                .Include(c => c.DetallesCompra)
                    .ThenInclude(d => d.Producto) // Productos dentro de los detalles
                .AsNoTracking()
                .FirstOrDefault(c => c.IdCompra == id);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task Crear(Compras compra)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Guardar la compra base primero
            compra.IdCompra = 0; // Force Identity Generation
            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();

            // 2. Procesar productos de forma aislada para evitar conflictos de tracking
            foreach (var detalle in compra.DetallesCompra)
            {
                // Usamos AsTracking() para asegurar que EF sepa que vamos a modificarlo
                var producto = await _context.Productos.AsTracking()
                                .FirstOrDefaultAsync(p => p.IdProducto == detalle.IdProducto);
                
                if (producto != null)
                {
                    // Actualizar Stock
                    decimal stockPrevio = producto.Stock;
                    decimal costoPrevio = producto.Costo;
                    
                    producto.Stock += detalle.Cantidad;

                    // Actualizar Costo (Ponderado)
                    if (stockPrevio + detalle.Cantidad > 0)
                    {
                        producto.Costo = ((stockPrevio * costoPrevio) + (detalle.Cantidad * detalle.CostoUnitario)) 
                                        / (stockPrevio + detalle.Cantidad);
                    }
                    else
                    {
                        producto.Costo = detalle.CostoUnitario;
                    }
                    
                    // Forzamos el estado a modificado
                    _context.Entry(producto).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Log exacto para la consola (aunque _logger sería ideal, Console sirve para dotnet watch rápido)
            Console.WriteLine($"FATAL DB ERROR: {ex.Message} Inner: {ex.InnerException?.Message}");
            throw;
        }
    }
    

    public void Actualizar(Compras compra)
    {
        try
        {
            _context.Compras.Update(compra);
            _context.SaveChanges();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void Eliminar(int id)
    {
        try
        {
            var compra = _context.Compras.Find(id);
            if (compra != null)
            {
                _context.Compras.Remove(compra);
                _context.SaveChanges();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

}