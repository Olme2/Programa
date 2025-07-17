using entornoPolleria;

public class MetodosPagoRepository : IMetodosPagoRepository
{
    private readonly AppDbContext _context;

    public MetodosPagoRepository(AppDbContext context)
    {
        _context = context;
    }

    public void CrearMetodoDePago(MetodosDePago metodoDePago)
    {
        _context.MetodosDePago.Add(metodoDePago);
        _context.SaveChanges();
    }
    
    public List<MetodosDePago> ListarMetodosDePagoRegistrados()
    {
        return _context.MetodosDePago.ToList();
    }

    public MetodosDePago ObtenerDetallesDeMetodoDePagoPorId(short id)
    {
        MetodosDePago? metodoDePago = null;
        metodoDePago = _context.MetodosDePago.Find(id);
        if (metodoDePago == null)
        {
            throw new Exception("Metodo de pago inexistente");
        }
        return metodoDePago;
    }

    public void ModificarMetodoDePago(MetodosDePago metodoDePago)
    {
        _context.MetodosDePago.Update(metodoDePago);
        _context.SaveChanges();
    }
    
    public void EliminarMetodoDePagoPorId(short id)
    {
        var metodoDePago = _context.MetodosDePago.Find(id);
        if (metodoDePago != null)
        {
            _context.MetodosDePago.Remove(metodoDePago);
            _context.SaveChanges();
        }
    }

} 