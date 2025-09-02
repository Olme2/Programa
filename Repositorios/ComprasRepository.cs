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

    public void Crear(Compras compra)
    {
        _context.Compras.Add(compra);
        _context.SaveChanges();
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