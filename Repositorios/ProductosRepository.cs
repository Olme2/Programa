using entornoPolleria;

public class ProductosRepository : IProductosRepository
{
    private readonly AppDbContext _context;

    public ProductosRepository(AppDbContext context)
    {
        _context = context;
    }

    public void CrearNuevoProducto(Productos producto)
    {
        _context.Productos.Add(producto);
        _context.SaveChanges();
    }

    public void ModificarProducto(Productos producto)
    {
        _context.Productos.Update(producto);
        _context.SaveChanges();
    }

    public List<Productos> ListarProductosRegistrados()
    {
        return _context.Productos.ToList();
    }

    public Productos ObtenerDetallesDeProductoPorId(int id)
    {
        var producto = _context.Set<Productos>().FirstOrDefault(p => p.IdProducto == id);
        if (producto == null)
        {
            throw new Exception("Producto inexistente");
        }
        return producto;
    }

    public void EliminarProductoPorId(int id)
    {
        var producto = _context.Productos.Find(id);
        if (producto != null)
        {
            _context.Productos.Remove(producto);
            _context.SaveChanges();
        }
    }
}