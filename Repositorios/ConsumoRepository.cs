using ConsumoVM;
using entornoPolleria;
using Microsoft.EntityFrameworkCore;

public class ConsumoRepository : IConsumoRepository
{
    private readonly AppDbContext _context;
    public ConsumoRepository(AppDbContext context) => _context = context;

    private short ObtenerIdMetodoYo()
    {
        // Usa "Yo" si existe, sino cualquier método disponible
        return _context.MetodosPago
            .OrderBy(m => m.Metodo == "Yo" ? 0 : 1)
            .ThenBy(m => m.IdMetodo)
            .Select(m => m.IdMetodo)
            .First();
    }

    public void Crear(AltaConsumoVM vm)
    {
        var fecha    = DateOnly.FromDateTime(vm.Fecha);
        var hora     = TimeOnly.FromDateTime(DateTime.Now);
        var idMetodo = ObtenerIdMetodoYo();

        var detalles = vm.Detalles
            .Where(d => d.IdProducto > 0 && d.Cantidad > 0)
            .Select(d => DetallesVentas.CrearParaEgreso(d.IdProducto, d.Cantidad, d.CostoUnitario))
            .ToList();

        var egreso = Ventas.CrearEgreso("consumo", idMetodo, fecha, hora, vm.Detalle, detalles);
        _context.Ventas.Add(egreso);
        _context.SaveChanges();
    }

    public void Actualizar(ModificarConsumoVM vm)
    {
        var egreso = _context.Ventas
            .Include(v => v.DetallesVenta)
            .FirstOrDefault(v => v.IdVenta == vm.IdConsumo && v.Tipo == "consumo");

        if (egreso == null)
            throw new InvalidOperationException("No se encontro el consumo.");

        using var transaction = _context.Database.BeginTransaction();

        egreso.Fecha = DateOnly.FromDateTime(vm.Fecha);
        egreso.Hora = vm.Hora;
        egreso.Detalle = vm.Detalle;

        _context.DetallesVentas.RemoveRange(egreso.DetallesVenta);
        _context.SaveChanges();

        var detalles = vm.Detalles
            .Where(d => d.IdProducto > 0 && d.Cantidad > 0)
            .Select(d => DetallesVentas.CrearParaEgreso(d.IdProducto, d.Cantidad, d.CostoUnitario))
            .ToList();

        foreach (var detalle in detalles)
        {
            detalle.IdVenta = egreso.IdVenta;
            _context.DetallesVentas.Add(detalle);
        }

        _context.SaveChanges();
        transaction.Commit();
    }

    public IEnumerable<ListarConsumoVM> ObtenerListado(DateTime fechaInicio, DateTime fechaFin)
    {
        var inicio = DateOnly.FromDateTime(fechaInicio);
        var fin    = DateOnly.FromDateTime(fechaFin);

        return _context.Ventas
            .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
            .Where(v => v.Tipo == "consumo" && v.Fecha >= inicio && v.Fecha <= fin)
            .OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Hora)
            .AsEnumerable()
            .Select(v => new ListarConsumoVM
            {
                IdConsumo  = (int)v.IdVenta,
                Fecha      = v.Fecha,
                Hora       = v.Hora,
                Detalle    = v.Detalle,
                Productos  = string.Join(", ", v.DetallesVenta.Select(d => d.Producto.Producto + " ×" + d.Cantidad.ToString("N3"))),
                TotalCosto = v.CalcularCostoTotal(),
            })
            .ToList();
    }

    public Ventas? ObtenerPorId(int id) =>
        _context.Ventas
            .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
            .FirstOrDefault(v => v.IdVenta == id && v.Tipo == "consumo");

    public void Eliminar(int id)
    {
        var egreso = _context.Ventas
            .Include(v => v.DetallesVenta)
            .FirstOrDefault(v => v.IdVenta == id && v.Tipo == "consumo");
        if (egreso == null) return;

        // Limpiar detalles primero para disparar triggers de stock
        _context.DetallesVentas.RemoveRange(egreso.DetallesVenta);
        _context.SaveChanges();
        _context.Ventas.Remove(egreso);
        _context.SaveChanges();
    }
}
