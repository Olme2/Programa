using entornoPolleria;
using Microsoft.EntityFrameworkCore;

public class VentaRepository : IVentaRepository
{
    private readonly AppDbContext _context;

    public VentaRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Ventas> ListarVentasRegistradas()
    {
        List<Ventas> ventas = _context.Set<Ventas>().Include(v => v.DetallesVenta).ToList();
        return ventas;
    }

    public Ventas ObtenerDetallesDeVentaPorId(long id)
    {
        return _context.Set<Ventas>().Find(id);
    }

    public void CrearNuevaVenta(Ventas venta)
    {
        _context.Set<Ventas>().Add(venta);
        _context.SaveChanges();
    }

    public void ModificarVenta(Ventas venta)
    {
        _context.Set<Ventas>().Update(venta);
        _context.SaveChanges();
    }

    public void EliminarVentaPorId(long id)
    {
        var detalles = _context.Set<DetallesVentas>().Where(d => d.IdVenta == id).ToList();
        _context.Set<DetallesVentas>().RemoveRange(detalles);
        var venta = _context.Set<Ventas>().Find(id);
        if (venta != null)
        {
            _context.Set<Ventas>().Remove(venta);
            _context.SaveChanges();
        }
    }

    public List<Ventas> ListarVentasEntreDosFechas(DateOnly fechaInicio, DateOnly fechaFin)
    {
        return _context.Set<Ventas>().Include(v => v.DetallesVenta).Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin).ToList();
    }
}

/*using entornoPolleria;

public interface IDetalleVentaRepository
{
    List<DetallesVentas> GetByVentaId(long idVenta);
    DetallesVentas GetById(long idVenta, int idProducto);
    void Add(DetallesVentas detalle);
    void Update(DetallesVentas detalle);
    void Delete(long idVenta, int idProducto);
}

public class DetalleVentaRepository : IDetalleVentaRepository
{
    private readonly AppDbContext _context;

    public DetalleVentaRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<DetallesVentas> GetByVentaId(long idVenta)
    {
        return _context.Set<DetallesVentas>().Where(d => d.IdVenta == idVenta).ToList();
    }

    public DetallesVentas GetById(long idVenta, int idProducto)
    {
        return _context.Set<DetallesVentas>().Find(idVenta, idProducto);
    }

    public void Add(DetallesVentas detalle)
    {
        _context.Set<DetallesVentas>().Add(detalle);
        _context.SaveChanges();
    }

    public void Update(DetallesVentas detalle)
    {
        _context.Set<DetallesVentas>().Update(detalle);
        _context.SaveChanges();
    }

    public void Delete(long idVenta, int idProducto)
    {
        var detalle = _context.Set<DetallesVentas>().Find(idVenta, idProducto);
        if (detalle != null)
        {
            _context.Set<DetallesVentas>().Remove(detalle);
            _context.SaveChanges();
        }
    }
}*/