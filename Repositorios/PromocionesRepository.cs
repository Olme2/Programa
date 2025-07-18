using entornoPolleria;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

public class PromocionesRepository : IPromocionesRepository
{
    private readonly AppDbContext _context;

    public PromocionesRepository(AppDbContext context)
    {
        _context = context;
    }

    public void CrearNuevaPromocion(Promociones promocion)
    {
        _context.Promociones.Add(promocion);
        _context.SaveChanges();
    }

    public void ModificarPromocion(Promociones promocion)
    {
        _context.Promociones.Update(promocion);
        _context.SaveChanges();
    }

    public List<Promociones> ListarPromocionesRegistradas()
{
    return _context.Set<Promociones>()
        .Include(p => p.DetallesPromocion)         
            .ThenInclude(d => d.Producto)
        .ToList();
}

    public Promociones ObtenerDetallesDePromocionPorId(int id)
    {
        var promocion = _context.Promociones.Include(p => p.DetallesPromocion).FirstOrDefault(p => p.IdPromocion == id);
        if (promocion == null)
        {
            throw new System.Exception("Promoción inexistente");
        }
        return promocion;
    }

    public void EliminarPromocionPorId(int id)
    {
        var promocion = _context.Promociones.Include(p => p.DetallesPromocion).FirstOrDefault(p => p.IdPromocion == id);
        if (promocion != null)
        {
            // Eliminar detalles primero si existen
            if (promocion.DetallesPromocion != null && promocion.DetallesPromocion.Count > 0)
            {
                _context.Set<DetallesPromociones>().RemoveRange(promocion.DetallesPromocion);
            }
            _context.Promociones.Remove(promocion);
            _context.SaveChanges();
        }
    }
} 