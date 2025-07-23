using Microsoft.EntityFrameworkCore;
using PromocionesVM;
using entornoPolleria;
// Asegúrate de que los 'using' apunten a tus carpetas correctas de Modelos y Repositorios

public class PromocionesRepository : IPromocionesRepository
{
    private readonly AppDbContext _context;
    public PromocionesRepository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<ListarPromocionesVM> ObtenerListadoPromociones()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);

        return _context.Promociones
            .Include(p => p.DetallesPromocion)
            .ThenInclude(d => d.Producto)
            .Select(p => new ListarPromocionesVM
            {
                IdPromocion = p.IdPromocion,
                Promocion = p.Promocion,
                Precio = p.Precio,
                Inicio = p.Inicio,
                Fin = p.Fin,
                Costo = p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad),
                ProductosConcatenados = string.Join("<br>", p.DetallesPromocion.Select(d => d.Producto.Producto)),
                Activa = !p.Fin.HasValue || p.Fin.Value > hoy,
                Ganancia = p.Precio - p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad),
                PorcentajeGanancia = (p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad) > 0) ? (p.Precio - p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad)) / p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad) : 0,
                EsEliminable = !_context.VentasPromociones.Any(vp => vp.IdPromocion == p.IdPromocion)
            })
            .ToList();
    }

    public Promociones? ObtenerPorId(int id)
    {
        return _context.Promociones.Include(p => p.DetallesPromocion).ThenInclude(d => d.Producto).FirstOrDefault(p => p.IdPromocion == id);
    }

    public void Crear(Promociones promocion)
    {
        _context.Promociones.Add(promocion);
        _context.SaveChanges();
    }

    public void Actualizar(Promociones promocion)
    {
        _context.Promociones.Update(promocion);
        _context.SaveChanges();
    }

    public void Eliminar(int id)
    {
        var promocion = _context.Promociones.Find(id);
        if (promocion != null)
        {
            _context.Promociones.Remove(promocion);
            _context.SaveChanges();
        }
    }
    public bool PuedeSerEliminada(int id)
    {
        return !_context.VentasPromociones.Any(vp => vp.IdPromocion == id);
    }
    public void DesactivarPorIdProducto(int id)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        var promocionesReferenciadas = _context.Promociones.Where(p =>(!p.Fin.HasValue || p.Fin.Value > hoy) && p.DetallesPromocion.Any(d => d.IdProducto == id));
        foreach (var promocion in promocionesReferenciadas)
        {
            if (promocion.Inicio > hoy)
            {
                _context.Remove(promocion);
            }
            else
            {
                promocion.Desactivar();
                _context.Update(promocion);                
            }
        }
        _context.SaveChanges();
    }
}