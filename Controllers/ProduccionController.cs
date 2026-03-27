using ProduccionVM;
using Microsoft.AspNetCore.Mvc;

public class ProduccionController : Controller
{
    private readonly IProduccionRepository _repo;
    private readonly IProductosRepository _productosRepo;
    private readonly ILogger<ProduccionController> _logger;

    public ProduccionController(IProduccionRepository repo, IProductosRepository productosRepo, ILogger<ProduccionController> logger)
    {
        _repo = repo;
        _productosRepo = productosRepo;
        _logger = logger;
    }

    // GET: Produccion/Index
    [HttpGet]
    public IActionResult Index(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var inicio = fechaInicio ?? DateTime.Today.AddDays(-30);
        var fin    = fechaFin    ?? DateTime.Today;
        var vm = new IndexProduccionVM
        {
            FechaInicio  = inicio,
            FechaFin     = fin,
            Producciones = _repo.ObtenerListado(inicio, fin).ToList(),
        };
        return View(vm);
    }

    // GET: Produccion/Alta
    [HttpGet]
    public IActionResult Alta()
    {
        var vm = new AltaProduccionVM
        {
            ProductosActivos = _productosRepo.ObtenerListadoProductos()
                .Where(p => p.Activo)
                .OrderBy(p => p.Producto)
                .ToList(),
        };
        return View(vm);
    }

    // POST: Produccion/Alta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaProduccionVM vm)
    {
        try
        {
            if (!vm.Detalles.Any(d => d.IdProducto > 0 && d.Cantidad > 0))
            {
                TempData["ErrorMessage"] = "Debe agregar al menos un producto.";
                return RedirectToAction(nameof(Alta));
            }
            _repo.Crear(vm);
            TempData["SuccessMessage"] = "Retiro de producción registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al registrar produccion");
            TempData["ErrorMessage"] = "Error al registrar: " + e.Message;
            return RedirectToAction(nameof(Alta));
        }
    }

    // POST: Produccion/Eliminar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        try
        {
            _repo.Eliminar(id);
            TempData["SuccessMessage"] = "Retiro eliminado. El stock fue devuelto.";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar produccion {Id}", id);
            TempData["ErrorMessage"] = "Error al eliminar: " + e.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
