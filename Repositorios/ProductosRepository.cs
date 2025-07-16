using entornoPolleria;
using Microsoft.EntityFrameworkCore;

class ProductosRepository : IProductosRepository{
    private readonly AppDbContext _context;
    public ProductosRepository(AppDbContext context){
        _context = context;
    }
    public void CrearNuevoProducto(Productos producto){
        _context.Set<Productos>().Add(producto);
        _context.SaveChanges();
    }
    public void ModificarProducto(Productos producto){
        _context.Set<Productos>().Update(producto);
        _context.SaveChanges();
    }
    public List<Productos> ListarProductosRegistrados(){
        return _context.Set<Productos>().ToList();
    }
    public Productos ObtenerDetallesDeProductoPorId(int id){
        var producto = _context.Set<Productos>().FirstOrDefault(p => p.IdProducto == id);
        if(producto == null){
            throw new Exception("Producto inexistente");
        }
        return producto;
    }
    public void EliminarProductoPorId(int id){
        var producto = _context.Set<Productos>().FirstOrDefault(p => p.IdProducto == id);
        if(producto != null){
            _context.Set<Productos>().Remove(producto);
            _context.SaveChanges();
        }
        // Eliminar detalles relacionados en PresupuestosDetalle si corresponde
        
    }
}