using entornoPolleria;

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
}