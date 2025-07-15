using entornoPolleria;
using Microsoft.EntityFrameworkCore;

class PresupuestosRepository : IPresupuestosRepository{
    private readonly AppDbContext _context;
    public PresupuestosRepository(AppDbContext context){
        _context = context;
    }
    public void CrearPresupuesto(Presupuestos presupuesto){
        _context.Set<Presupuestos>().Add(presupuesto);
        _context.SaveChanges();
    }
    public Presupuestos ObtenerPresupuestoPorId(int id)
    {
        var presupuesto = _context.Set<Presupuestos>()
            .Include(p => p.Detalle)
            .ThenInclude(d => d.Producto)
            .FirstOrDefault(p => p.IdPresupuesto == id);
        if (presupuesto == null)
        {
            throw new Exception("Presupuesto inexistente");
        }
        return presupuesto;
    }
    public void AgregarProducto(int idPresupuesto, int idProducto, int cantidad){
        var presupuesto = _context.Set<Presupuestos>()
            .Include(p => p.Detalle)
            .FirstOrDefault(p => p.IdPresupuesto == idPresupuesto);
        var producto = _context.Set<Productos>().FirstOrDefault(p => p.IdProducto == idProducto);
        if(presupuesto != null && producto != null){
            var detalle = presupuesto.Detalle.FirstOrDefault(d => d.Producto.IdProducto == idProducto);
            if(detalle != null){
                detalle.Cantidad += cantidad;
            }else{
                presupuesto.Detalle.Add(new PresupuestosDetalle(producto, cantidad));
            }
            _context.SaveChanges();
        }
    }
    public void EliminarPresupuestoPorId(int id){
        var presupuesto = _context.Set<Presupuestos>().FirstOrDefault(p => p.IdPresupuesto == id);
        if(presupuesto != null){
            _context.Set<Presupuestos>().Remove(presupuesto);
            _context.SaveChanges();
        }
    }
    public List<PresupuestosDetalle> MostrarDetallePorId(int id){
        var presupuesto = _context.Set<Presupuestos>()
            .Include(p => p.Detalle)
            .ThenInclude(d => d.Producto)
            .FirstOrDefault(p => p.IdPresupuesto == id);
        return presupuesto?.Detalle ?? new List<PresupuestosDetalle>();
    }
    public void EliminarProducto(int idPresupuesto, int idProducto, int cantVieja, int cantNueva){
        var presupuesto = _context.Set<Presupuestos>()
            .Include(p => p.Detalle)
            .FirstOrDefault(p => p.IdPresupuesto == idPresupuesto);
        if(presupuesto != null){
            var detalle = presupuesto.Detalle.FirstOrDefault(d => d.Producto.IdProducto == idProducto);
            if(detalle != null){
                if(cantNueva == 0){
                    presupuesto.Detalle.Remove(detalle);
                }else{
                    detalle.Cantidad = cantNueva;
                }
                _context.SaveChanges();
            }
        }
    }
    public void ModificarPresupuesto(Presupuestos presupuesto){
        _context.Set<Presupuestos>().Update(presupuesto);
        _context.SaveChanges();
    }
}