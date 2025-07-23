using entornoPolleria;
using MetodosVM;
using Microsoft.EntityFrameworkCore;

public class MetodosPagoRepository : IMetodosPagoRepository
{
    private readonly AppDbContext _context;

    public MetodosPagoRepository(AppDbContext context)
    {
        _context = context;
    }
    public IEnumerable<ListarMetodosPagoVM> ObtenerListadoMetodosPago()
    {
        return _context.MetodosPago
        .Select(m => new ListarMetodosPagoVM
        {
            IdMetodo = m.IdMetodo,
            Metodo = m.Metodo,
            EsEliminable = !_context.Ventas.Any(v => v.IdMetodo == m.IdMetodo)
        })
        .OrderBy(m => m.Metodo)
        .ToList();
    }

    public void Crear(MetodosPago metodoPago)
    {
        _context.MetodosPago.Add(metodoPago);
        _context.SaveChanges();
    }

    public MetodosPago? ObtenerPorId(short id)
    {
        return _context.MetodosPago.FirstOrDefault(m => m.IdMetodo == id);
    }

    public void Actualizar(MetodosPago metodoPago)
    {
        _context.MetodosPago.Update(metodoPago);
        _context.SaveChanges();
    }

    public void Eliminar(short id)
    {
        var metodoPago = _context.MetodosPago.Find(id);
        if (metodoPago != null)
        {
            _context.MetodosPago.Remove(metodoPago);
            _context.SaveChanges();
        }
    }

    public bool PuedeSerEliminado(short id){
        return !_context.Ventas.Any(v => v.IdMetodo == id);
    }
} 