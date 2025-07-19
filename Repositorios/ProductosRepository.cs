using Microsoft.EntityFrameworkCore;
using entornoPolleria;
using ProductosVM;
public class ProductosRepository : IProductosRepository
{
    private readonly AppDbContext _context;

    public ProductosRepository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<ListarProductosVM> ObtenerListadoProductos()
    {
        var unaSemanaAtras = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));

        // Esta consulta se traduce a SQL y se ejecuta en la base de datos.
        return _context.Productos.Include(p => p.Proveedor).Select(p => new ListarProductosVM
        {
            IdProducto = p.IdProducto,
            Producto = p.Producto,
            Proveedor = p.Proveedor.Proveedor,
            Stock = p.Stock,
            Costo = p.Costo,
            Precio = p.Precio,
            Activo = p.Activo,
            Ganancia = p.Precio - p.Costo,
            PorcentajeGanancia = (p.Costo > 0) ? ((p.Precio - p.Costo) / p.Costo) : 0,
            // Calculamos si el producto está referenciado en alguna otra tabla
            EsEliminable = !_context.DetallesVentas.Any(dv => dv.IdProducto == p.IdProducto) &&
                               !_context.DetallesPromociones.Any(dp => dp.IdProducto == p.IdProducto) &&
                               !_context.DetallesCompras.Any(dc => dc.IdProducto == p.IdProducto),
            // Sumamos la cantidad de productos vendidos en la última semana
            VendidosSemana = _context.DetallesVentas.Where(dv => dv.IdProducto == p.IdProducto && dv.Venta.Fecha >= unaSemanaAtras).Sum(dv => (int)dv.Cantidad)
        })
            .ToList();
    }

    public Productos? ObtenerPorId(int id)
    {
        return _context.Productos.Include(p => p.Proveedor).FirstOrDefault(p => p.IdProducto == id);
    }

    public void Crear(Productos producto)
    {
        _context.Productos.Add(producto);
        _context.SaveChanges();
    }

    public void Actualizar(Productos producto)
    {
        _context.Productos.Update(producto);
        _context.SaveChanges();
    }

    public void Eliminar(int id)
    {
        var producto = _context.Productos.Find(id);
        if (producto != null)
        {
            _context.Productos.Remove(producto);
            _context.SaveChanges();
        }
    }
    public bool PuedeSerEliminado(int id)
    {
        bool enVentas = _context.DetallesVentas.Any(dv => dv.IdProducto == id);
        bool enPromociones = _context.DetallesPromociones.Any(dp => dp.IdProducto == id);
        bool enCompras = _context.DetallesCompras.Any(dc => dc.IdProducto == id);

        return !enVentas && !enPromociones && !enCompras;
    }
}