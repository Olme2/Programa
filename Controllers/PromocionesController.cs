using Microsoft.AspNetCore.Mvc;
using PromocionesVM;
using DetallesPromocionesVM;

public class PromocionesController : Controller
{
    private readonly ILogger<PromocionesController> _logger;
    private readonly IPromocionesRepository _promocionesRepo;
    private readonly IProductosRepository _productosRepo;

    public PromocionesController(ILogger<PromocionesController> logger, IPromocionesRepository promocionesRepo, IProductosRepository productosRepo)
    {
        _logger = logger;
        _promocionesRepo = promocionesRepo;
        _productosRepo = productosRepo;
    }

    [HttpGet]
    public IActionResult Index()
    {
        try
        {
            var promocionesVM = _promocionesRepo.ObtenerListadoPromociones();
            return View(promocionesVM.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el listado de promociones.");
            TempData["ErrorMessage"] = "No se pudo cargar el listado de promociones.";
            return View(new List<ListarPromocionesVM>());
        }
    }

    [HttpGet]
    public IActionResult Alta()
    {
        var viewModel = new AltaPromocionVM();
        viewModel.DetallesPromocion.Add(new AltaDetallePromocionVM());
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaPromocionVM viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var nuevaPromocion = Promociones.CrearDesdeViewModel(viewModel);
            _promocionesRepo.Crear(nuevaPromocion);
            TempData["SuccessMessage"] = "Promoción creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear la promoción.");
            TempData["ErrorMessage"] = "Ocurrió un error al crear la promoción.";
            return View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult Modificar(int id)
    {
        try
        {
            var promocion = _promocionesRepo.ObtenerPorId(id);
            if (promocion == null)
            {
                return NotFound();
            }
            var viewModel = new ModificarPromocionVM(promocion);
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener la promoción con ID {PromocionId} para modificar.", id);
            TempData["ErrorMessage"] = "No se pudo cargar la promoción para modificar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(ModificarPromocionVM viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var promocion = _promocionesRepo.ObtenerPorId(viewModel.IdPromocion);
            if (promocion == null)
            {
                return NotFound();
            }

            promocion.ActualizarDesdeViewModel(viewModel);
            _promocionesRepo.Actualizar(promocion);
            TempData["SuccessMessage"] = "Promoción modificada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar la promoción con ID {PromocionId}", viewModel.IdPromocion);
            TempData["ErrorMessage"] = "Ocurrió un error al modificar la promoción.";
            return View(viewModel);
        }
    }

    [HttpGet]
    public IActionResult Eliminar(int id)
    {
        try
        {
            var promocionVM = _promocionesRepo.ObtenerListadoPromociones().FirstOrDefault(p => p.IdPromocion == id);
            if (promocionVM == null)
            {
                return NotFound();
            }
            return View(promocionVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener la promoción con ID {PromocionId} para eliminar.", id);
            TempData["ErrorMessage"] = "No se pudo cargar la promoción para eliminar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarConfirmado(int id)
    {
        try
        {
            if (!_promocionesRepo.PuedeSerEliminada(id))
            {
                TempData["ErrorMessage"] = "No se puede eliminar la promoción porque ya ha sido registrada en una o más ventas.";
                return RedirectToAction(nameof(Index));
            }

            _promocionesRepo.Eliminar(id);
            TempData["SuccessMessage"] = "Promoción eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar la promoción con ID {PromocionId}", id);
            TempData["ErrorMessage"] = "Ocurrió un error al eliminar la promoción.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult _BuscarPromociones(string busqueda)
    {
        try
        {
            var promocionesVM = _promocionesRepo.ObtenerListadoPromociones();
            if (!string.IsNullOrEmpty(busqueda))
            {
                promocionesVM = promocionesVM.Where(p =>
                    p.Promocion.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase) ||
                    p.ProductosConcatenados.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase)
                );
            }
            var promocionesOrdenadas = promocionesVM.OrderByDescending(p => p.Activa).ThenByDescending(p => p.Inicio);
            ViewData["BusquedaActual"] = busqueda;
            return PartialView("_PromocionesTabla", promocionesOrdenadas.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en la búsqueda dinámica de promociones.");
            return StatusCode(500);
        }
    }

    [HttpGet]
    public IActionResult BuscarProductosParaPromocion(string term, [FromQuery] int[] excluir)
    {
        try
        {
            var query = _productosRepo.ObtenerListadoProductos().Where(p => p.Activo);
            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(p => p.Producto.Contains(term, StringComparison.CurrentCultureIgnoreCase));
            }
            if (excluir != null && excluir.Length > 0)
            {
                query = query.Where(p => !excluir.Contains(p.IdProducto));
            }
            var productos = query.OrderBy(p => p.Producto)
                .Select(p => new {
                    id = p.IdProducto,
                    text = p.Producto,
                    costo = p.Costo
                })
                .Take(10)
                .ToList();
            return Json(productos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en la búsqueda de productos para promoción.");
            return StatusCode(500);
        }
    }

    [HttpGet]
    public IActionResult ObtenerVistaDetallePromocion(int index)
    {
        ViewData["index"] = index;
        return PartialView("Views/Shared/_DetallePromocionItem.cshtml", new AltaDetallePromocionVM());
    }
}