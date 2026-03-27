using ProduccionVM;
using entornoPolleria;
using Microsoft.EntityFrameworkCore;

public class ProduccionRepository : IProduccionRepository
{
    private readonly AppDbContext _context;
    public ProduccionRepository(AppDbContext context) => _context = context;

    private short ObtenerIdMetodoYo()
    {
        return _context.MetodosPago
            .OrderBy(m => m.Metodo == "Yo" ? 0 : 1)
            .ThenBy(m => m.IdMetodo)
            .Select(m => m.IdMetodo)
            .First();
    }

    public void Crear(AltaProduccionVM vm)
    {
        var fecha    = DateOnly.FromDateTime(vm.Fecha);
        var hora     = TimeOnly.FromDateTime(DateTime.Now);
        var idMetodo = ObtenerIdMetodoYo();

        var detalles = vm.Detalles
            .Where(d => d.IdProducto > 0 && d.Cantidad > 0)
            .Select(d => DetallesVentas.CrearParaEgreso(d.IdProducto, d.Cantidad, d.CostoUnitario))
            .ToList();


        var egreso = Ventas.CrearEgreso("produccion", idMetodo, fecha, hora, vm.Detalle, detalles);
        _context.Ventas.Add(egreso);
        _context.SaveChanges();
    }

    public IEnumerable<ListarProduccionVM> ObtenerListado(DateTime fechaInicio, DateTime fechaFin)
    {
        var inicio = DateOnly.FromDateTime(fechaInicio);
        var fin    = DateOnly.FromDateTime(fechaFin);

        return _context.Ventas
            .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
            .Where(v => v.Tipo == "produccion" && v.Fecha >= inicio && v.Fecha <= fin)
            .OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Hora)
            .AsEnumerable()
            .Select(v => new ListarProduccionVM
            {
                IdProduccion = (int)v.IdVenta,
                Fecha        = v.Fecha,
                Hora         = v.Hora,
                Detalle      = v.Detalle,
                Productos    = string.Join(", ", v.DetallesVenta.Select(d => d.Producto.Producto + " ×" + d.Cantidad.ToString("N3"))),
            })
            .ToList();
    }

    public Ventas? ObtenerPorId(int id) =>
        _context.Ventas
            .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
            .FirstOrDefault(v => v.IdVenta == id && v.Tipo == "produccion");

    public void Eliminar(int id)
    {
        var egreso = _context.Ventas
            .Include(v => v.DetallesVenta)
            .FirstOrDefault(v => v.IdVenta == id && v.Tipo == "produccion");
        if (egreso == null) return;

        _context.DetallesVentas.RemoveRange(egreso.DetallesVenta);
        _context.SaveChanges();
        _context.Ventas.Remove(egreso);
        _context.SaveChanges();
    }
}
