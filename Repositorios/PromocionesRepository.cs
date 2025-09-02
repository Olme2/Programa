using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Html;
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
        var unaSemanaAtras = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
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
                ProductosConcatenados = new HtmlString(string.Join("<br>", p.DetallesPromocion.Select(d => d.Producto.Producto + " x" + d.Cantidad.ToString("N0")))),
                Activa = !p.Fin.HasValue || p.Fin.Value > hoy,
                Ganancia = p.Precio - p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad),
                PorcentajeGanancia = (p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad) > 0) ? (p.Precio - p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad)) / p.DetallesPromocion.Sum(d => d.Producto.Costo * d.Cantidad) : 0,
                EsEliminable = !_context.VentasPromociones.Any(vp => vp.IdPromocion == p.IdPromocion),
                Stock = p.DetallesPromocion
                        .Where(d => d.Cantidad > 0)
                        .Select(d => d.Producto.Stock / d.Cantidad)
                        .Min(),
                VentaSemanal = _context.VentasPromociones
                                .Where(vp => vp.IdPromocion == p.IdPromocion &&
                                _context.Ventas.Any(v => v.IdVenta == vp.IdVenta && v.Fecha >= unaSemanaAtras && v.Fecha <= hoy))
                                .Sum(dv => (decimal?)dv.Cantidad) ?? 0,
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
    public IEnumerable<(string Nombre, decimal Precio)> ObtenerPromocionesActivasParaLista()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        return _context.Promociones
            .Where(p =>p.Inicio <= hoy && (p.Fin == null || p.Fin >= hoy))
            .OrderBy(p => p.Promocion)
            .Select(p => new ValueTuple<string, decimal>(p.Promocion, p.Precio))
            .ToList();
    }
}