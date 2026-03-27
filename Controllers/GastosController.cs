using GastosVM;
using Microsoft.AspNetCore.Mvc;

public class GastosController : Controller
{
    private readonly IGastosRepository _repo;
    private readonly ILogger<GastosController> _logger;

    public GastosController(IGastosRepository repo, ILogger<GastosController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var inicio = fechaInicio ?? DateTime.Today.AddDays(-30);
        var fin    = fechaFin    ?? DateTime.Today;
        var vm = new IndexGastosVM
        {
            FechaInicio = inicio,
            FechaFin    = fin,
            Gastos      = _repo.ObtenerListado(inicio, fin).ToList(),
        };
        return View(vm);
    }

    [HttpGet]
    public IActionResult Alta() => View(new AltaGastoVM());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaGastoVM vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            _repo.Crear(vm);
            TempData["SuccessMessage"] = "Gasto registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al registrar gasto");
            TempData["ErrorMessage"] = "Error: " + e.Message;
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        try
        {
            _repo.Eliminar(id);
            TempData["SuccessMessage"] = "Gasto eliminado.";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar gasto {Id}", id);
            TempData["ErrorMessage"] = "Error al eliminar: " + e.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
