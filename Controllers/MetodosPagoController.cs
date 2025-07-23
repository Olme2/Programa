using MetodosVM;
using Microsoft.AspNetCore.Mvc;
public class MetodosPagoController : Controller
{
    private readonly ILogger<ProductosController> _logger;
    private readonly IMetodosPagoRepository _metodosPagoRepo;
    public MetodosPagoController(ILogger<ProductosController> logger, IMetodosPagoRepository metodosPagoRepository)
    {
        _logger = logger;
        _metodosPagoRepo = metodosPagoRepository;
    }
    public IActionResult Index()
    {
        try
        {
            var metodosPagoVM = _metodosPagoRepo.ObtenerListadoMetodosPago();
            return View(metodosPagoVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el listado de métodos de pago.");
            TempData["ErrorMessage"] = "No se pudo cargar la lista de métodos de pago.";
            return View(new List<ListarMetodosPagoVM>());
        }
    }
    
    [HttpGet]
    public IActionResult Alta()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaMetodoPagoVM viewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
            var metodoPago = MetodosPago.CrearDesdeViewModel(viewModel);
            _metodosPagoRepo.Crear(metodoPago);
            TempData["SuccessMessage"] = "Método de pago creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear el nuevo método de pago.");
            TempData["ErrorMessage"] = "Ocurrió un error al crear el método de pago.";
            return View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult Modificar(short id)
    {
        try
        {
            var metodoPago = _metodosPagoRepo.ObtenerPorId(id);
            if (metodoPago == null)
            {
                TempData["ErrorMessage"] = "No existe método de pago con ese id.";
                return RedirectToAction(nameof(Index));
            }
            var viewModel = new ModificarMetodoPago(metodoPago);
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el método de pago con ID {IdMetodo} para modificar.", id);
            TempData["ErrorMessage"] = "No se pudo cargar el método de pago para modificar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public IActionResult Modificar(ModificarMetodoPago viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }
        try
        {
            var metodoPago = _metodosPagoRepo.ObtenerPorId(viewModel.IdMetodo);
            if (metodoPago == null)
            {
                TempData["ErrorMessage"] = "No existe método de pago con ese id.";
                return RedirectToAction(nameof(Index));
            }
            metodoPago.ActualizarDesdeViewModel(viewModel);
            _metodosPagoRepo.Actualizar(metodoPago);
            TempData["SuccessMessage"] = "Método de pago modificado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar el método de pago con ID {Id}", viewModel.IdMetodo);
            TempData["ErrorMessage"] = "Ocurrió un error al modificar el método de pago.";
            return View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult Eliminar(short id)
    {
        try
        {
            var metodoPagoVM = _metodosPagoRepo.ObtenerListadoMetodosPago().FirstOrDefault(m => m.IdMetodo == id);
            if (metodoPagoVM == null)
            {
                TempData["ErrorMessage"] = "No existe método de pago con ese id.";
                return RedirectToAction(nameof(Index));
            }
            if (!_metodosPagoRepo.PuedeSerEliminado(id))
            {
                TempData["ErrorMessage"] = "No se puede eliminar el metodo porque está en uso en ventas.";
                return RedirectToAction(nameof(Index));
            }
            return View(metodoPagoVM);
        }
        catch (Exception e)
        {
             _logger.LogError(e, "Error al obtener el método con ID {IdMetodo} para eliminar.", id);
            TempData["ErrorMessage"] = "No se pudo cargar el producto para eliminar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarConfirmado(short id)
    {
        try
        {
            _metodosPagoRepo.Eliminar(id);
            TempData["SuccessMessage"] = "Método de pago eliminado correctamente.";
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
           _logger.LogError(e, "Error al eliminar el método de pago con ID {Id}", id);
            TempData["ErrorMessage"] = "Ocurrió un error al eliminar el método de pago.";
            return RedirectToAction(nameof(Index));
        }
    }

}