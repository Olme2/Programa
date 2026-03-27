using GastosVM;
using entornoPolleria;

public class GastosRepository : IGastosRepository
{
    private readonly AppDbContext _context;
    public GastosRepository(AppDbContext context) => _context = context;

    public void Crear(AltaGastoVM vm)
    {
        var gasto = new Gasto(
            DateOnly.FromDateTime(vm.Fecha),
            vm.Nombre,
            vm.Monto,
            vm.Observacion);
        _context.Gastos.Add(gasto);
        _context.SaveChanges();
    }

    public IEnumerable<ListarGastoVM> ObtenerListado(DateTime fechaInicio, DateTime fechaFin)
    {
        var inicio = DateOnly.FromDateTime(fechaInicio);
        var fin    = DateOnly.FromDateTime(fechaFin);
        return _context.Gastos
            .Where(g => g.Fecha >= inicio && g.Fecha <= fin)
            .OrderByDescending(g => g.Fecha)
            .Select(g => new ListarGastoVM
            {
                IdGasto     = g.IdGasto,
                Fecha       = g.Fecha,
                Nombre      = g.Nombre,
                Monto       = g.Monto,
                Observacion = g.Observacion,
            })
            .ToList();
    }

    public void Eliminar(int id)
    {
        var gasto = _context.Gastos.Find(id);
        if (gasto == null) return;
        _context.Gastos.Remove(gasto);
        _context.SaveChanges();
    }
}
