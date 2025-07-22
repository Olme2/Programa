using Microsoft.AspNetCore.Mvc;
using ProveedoresVM;

public class ProveedoresController : Controller
{
    private readonly ILogger<ProveedoresController> _logger;
    private readonly IProveedoresRepository _proveedoresRepo;

    public ProveedoresController(ILogger<ProveedoresController> logger, IProveedoresRepository proveedoresRepo)
    {
        _logger = logger;
        _proveedoresRepo = proveedoresRepo;
    }

    [HttpGet]
    public IActionResult Index()
    {
        try
        {
            var proveedoresVM = _proveedoresRepo.ObtenerListadoProveedores();
            return View(proveedoresVM.OrderByDescending(p => p.Debo).ThenBy(p => p.Proveedor).ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el listado de proveedores.");
            TempData["ErrorMessage"] = "No se pudo cargar el listado de proveedores.";
            return View(new List<ListarProveedoresVM>());
        }
    }

    [HttpGet]
    public IActionResult Alta()
    {
        return View(new AltaProveedorVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaProveedorVM viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var proveedor = Proveedores.CrearDesdeViewModel(viewModel);
            _proveedoresRepo.Crear(proveedor);
            TempData["SuccessMessage"] = "Proveedor creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear el proveedor.");
            TempData["ErrorMessage"] = "Ocurrió un error al crear el proveedor.";
            return View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult Modificar(int id)
    {
        try
        {
            var proveedor = _proveedoresRepo.ObtenerPorId(id);
            if (proveedor == null)
            {
                TempData["ErrorMessage"] = "No existe proveedor con ese id.";
                return RedirectToAction(nameof(Index));
            }
            var viewModel = new ModificarProveedorVM(proveedor);
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el proveedor con ID {ProveedorId} para modificar.", id);
            TempData["ErrorMessage"] = "No se pudo cargar el proveedor para modificar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(ModificarProveedorVM viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var proveedor = _proveedoresRepo.ObtenerPorId(viewModel.IdProveedor);
            if (proveedor == null)
            {
                TempData["ErrorMessage"] = "No existe proveedor con ese id.";
                return RedirectToAction(nameof(Index));
            }

            proveedor.ActualizarDesdeViewModel(viewModel);
            _proveedoresRepo.Actualizar(proveedor);
            TempData["SuccessMessage"] = "Proveedor modificado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar el proveedor con ID {ProveedorId}", viewModel.IdProveedor);
            TempData["ErrorMessage"] = "Ocurrió un error al modificar el proveedor.";
            return View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult Eliminar(int id)
    {
        try
        {
            var proveedorVM = _proveedoresRepo.ObtenerListadoProveedores().FirstOrDefault(p => p.IdProveedor == id);
            if (proveedorVM == null)
            {
                TempData["ErrorMessage"] = "No existe proveedor con ese id.";
                return RedirectToAction(nameof(Index));
            }
            if (!proveedorVM.EsEliminable)
            {
                TempData["ErrorMessage"] = "No se puede eliminar el proveedor " + proveedorVM.Proveedor + ", es referenciado en los productos " + proveedorVM.ProductosReferenciados + ".";
                return RedirectToAction(nameof(Index));
            }
            if (proveedorVM.Debo > 0)
            {
                TempData["ErrorMessage"] = "No se puede eliminar el proveedor " + proveedorVM.Proveedor + " ya que todavia tiene una deuda.";
                return RedirectToAction(nameof(Index));
            }
            return View(proveedorVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el proveedor con ID {ProveedorId} para eliminar.", id);
            TempData["ErrorMessage"] = "No se pudo cargar el proveedor para eliminar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarConfirmado(int id)
    {
        try
        {
            _proveedoresRepo.Eliminar(id);
            TempData["SuccessMessage"] = "Proveedor eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar el proveedor con ID {ProveedorId}", id);
            TempData["ErrorMessage"] = "Ocurrió un error al eliminar el proveedor. Es posible que esté en uso.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult _BuscarProveedores(string busqueda, string filtroDeuda)
    {
        try
        {
            var proveedoresVM = _proveedoresRepo.ObtenerListadoProveedores();

            if (!string.IsNullOrEmpty(busqueda))
            {
                proveedoresVM = proveedoresVM.Where(p =>
                    p.Proveedor.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase));
            }
            switch (filtroDeuda)
            {
                case "conDeuda":
                    proveedoresVM = proveedoresVM.Where(p => p.Debo > 0);
                    break;
                case "sinDeuda":
                    proveedoresVM = proveedoresVM.Where(p => p.Debo == 0);
                    break;
            }

            var proveedoresOrdenados = proveedoresVM.OrderByDescending(p=> p.Debo).ThenBy(p => p.Proveedor);
            ViewData["BusquedaActual"] = busqueda;
            return PartialView("_ProveedoresTabla", proveedoresOrdenados.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en la búsqueda dinámica de proveedores.");
            return StatusCode(500);
        }
    }

    [HttpGet]
    public IActionResult BuscarProveedores(string term)
    {
        // Esta acción es para AJAX (Select2), por lo que no necesita TempData.
        try
        {
            var query = _proveedoresRepo.ObtenerListadoProveedores();
            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(p => p.Proveedor.Contains(term, StringComparison.CurrentCultureIgnoreCase));
            }
            var proveedores = query
                .OrderBy(p => p.Proveedor)
                .Select(p => new { id = p.IdProveedor, text = p.Proveedor })
                .ToList();
            return Json(proveedores);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en la búsqueda de proveedores para autocomplete.");
            return StatusCode(500);
        }
    }
}