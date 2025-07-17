using entornoPolleria;

public class VentaRepository : IVentaRepository
{
    private readonly AppDbContext _context;

    public VentaRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Ventas> ListarVentasRegistradas()
    {
        return _context.Set<Ventas>().ToList();
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
        return _context.Set<Ventas>()
            .Where(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin)
            .ToList();
    }
}