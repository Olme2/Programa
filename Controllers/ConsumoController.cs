using ConsumoVM;
using Microsoft.AspNetCore.Mvc;

public class ConsumoController : Controller
{
    private readonly IConsumoRepository _repo;
    private readonly IProductosRepository _productosRepo;
    private readonly ILogger<ConsumoController> _logger;

    public ConsumoController(IConsumoRepository repo, IProductosRepository productosRepo, ILogger<ConsumoController> logger)
    {
        _repo = repo;
        _productosRepo = productosRepo;
        _logger = logger;
    }

    // GET: Consumo/Index
    [HttpGet]
    public IActionResult Index(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var inicio = fechaInicio ?? DateTime.Today.AddDays(-30);
        var fin    = fechaFin    ?? DateTime.Today;
        var vm = new IndexConsumoVM
        {
            FechaInicio = inicio,
            FechaFin    = fin,
            Consumos    = _repo.ObtenerListado(inicio, fin).ToList(),
        };
        return View(vm);
    }

    // GET: Consumo/Alta
    [HttpGet]
    public IActionResult Alta()
    {
        var vm = new AltaConsumoVM
        {
            ProductosActivos = _productosRepo.ObtenerListadoProductos()
                .Where(p => p.Activo)
                .OrderBy(p => p.Producto)
                .ToList(),
        };
        return View(vm);
    }

    // POST: Consumo/Alta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaConsumoVM vm)
    {
        try
        {
            if (!vm.Detalles.Any(d => d.IdProducto > 0 && d.Cantidad > 0))
            {
                TempData["ErrorMessage"] = "Debe agregar al menos un producto.";
                return RedirectToAction(nameof(Alta));
            }
            _repo.Crear(vm);
            TempData["SuccessMessage"] = "Consumo registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al registrar consumo");
            TempData["ErrorMessage"] = "Error al registrar el consumo: " + e.Message;
            return RedirectToAction(nameof(Alta));
        }
    }

    // POST: Consumo/Eliminar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        try
        {
            _repo.Eliminar(id);
            TempData["SuccessMessage"] = "Consumo eliminado. El stock fue devuelto.";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar consumo {Id}", id);
            TempData["ErrorMessage"] = "Error al eliminar: " + e.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // GET AJAX: Consumo/ObtenerCostoProducto?idProducto=5
    [HttpGet]
    public IActionResult ObtenerCostoProducto(int idProducto)
    {
        var p = _productosRepo.ObtenerPorId(idProducto);
        if (p == null) return NotFound();
        return Json(new { costo = p.Costo, stock = p.Stock });
    }
}
