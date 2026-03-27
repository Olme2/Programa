using Microsoft.AspNetCore.Mvc;
using VM = DashboardVM;

public class DashboardController : Controller
{
    private readonly IDashboardRepository _repo;

    public DashboardController(IDashboardRepository repo) => _repo = repo;

    [HttpGet]
    public IActionResult Index(string periodo = "mes", DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        var (inicio, fin) = ResolverPeriodo(periodo, fechaInicio, fechaFin);
        var vm = _repo.ObtenerDashboard(inicio, fin, periodo);
        return View(vm);
    }

    private static (DateTime inicio, DateTime fin) ResolverPeriodo(string periodo, DateTime? customInicio, DateTime? customFin)
    {
        var hoy = DateTime.Today;
        return periodo switch
        {
            "semana"   => (hoy.AddDays(-6), hoy),
            "semestre" => (hoy.AddMonths(-6), hoy),
            "anio"     => (hoy.AddYears(-1), hoy),
            "custom"   => (customInicio ?? hoy.AddDays(-30), customFin ?? hoy),
            _          => (hoy.AddMonths(-1), hoy), // "mes" por defecto
        };
    }
}
