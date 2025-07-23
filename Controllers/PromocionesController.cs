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
            var promocionesVM = _promocionesRepo.ObtenerListadoPromociones().Where(p => p.Activa).OrderByDescending(p => p.Inicio).ThenBy(p => p.Promocion);
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
            // Esta validación se mantiene para los [Required], [Range], etc.
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        // --- VALIDACIÓN MANUAL DE PRECIO vs COSTO (AQUÍ ESTÁ LA SOLUCIÓN) ---
        try
        {
            // 1. Calcular el costo total real basado en los datos de la BD.
            decimal costoTotalReal = 0;
            if (viewModel.DetallesPromocion != null)
            {
                foreach (var detalleVM in viewModel.DetallesPromocion)
                {
                    var producto = _productosRepo.ObtenerPorId(detalleVM.IdProducto);
                    if (producto != null)
                    {
                        costoTotalReal += producto.Costo * detalleVM.Cantidad;
                    }
                }
            }

            // 2. Comparar y agregar el error de modelo si es necesario.
            if (viewModel.Precio < costoTotalReal)
            {
                ModelState.AddModelError("Precio", $"El precio no puede ser menor que el costo total (${costoTotalReal:N2}).");
            }

            // 3. Si agregamos nuestro error personalizado, volvemos a la vista.
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al validar el costo de la nueva promoción.");
            TempData["ErrorMessage"] = "Ocurrió un error inesperado al validar los datos.";
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
                TempData["ErrorMessage"] = "No existe promocion con ese id.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ModificarPromocionVM(promocion);

            // Pasamos la regla de negocio a la vista.
            viewModel.EsModificable = _promocionesRepo.PuedeSerEliminada(id);

            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al cargar el formulario de modificación para la promoción ID {id}", id);
            TempData["ErrorMessage"] = "Ocurrió un error al cargar la promoción.";
            return RedirectToAction(nameof(Index));
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(ModificarPromocionVM viewModel)
    {
        // Validación de atributos del ViewModel ([Required], etc.)
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Por favor, corrige los errores e intenta de nuevo.";
            // Repoblamos el VM para que la vista se muestre correctamente
            RepoblarViewModelParaModificar(viewModel);
            return View(viewModel);
        }

        try
        {
            var promocion = _promocionesRepo.ObtenerPorId(viewModel.IdPromocion);
            if (promocion == null)
            {
                TempData["ErrorMessage"] = "No existe promocion con ese id.";
                return RedirectToAction(nameof(Index));
            }

            // --- VALIDACIÓN MANUAL DE PRECIO vs COSTO ---
            decimal costoTotalReal = 0;
            bool esModificable = _promocionesRepo.PuedeSerEliminada(viewModel.IdPromocion);

            if (esModificable)
            {
                // Si es modificable, calculamos el costo con los nuevos datos del ViewModel
                if (viewModel.DetallesPromocion != null)
                {
                    foreach (var detalleVM in viewModel.DetallesPromocion)
                    {
                        var producto = _productosRepo.ObtenerPorId(detalleVM.IdProducto);
                        if (producto != null)
                        {
                            costoTotalReal += producto.Costo * detalleVM.Cantidad;
                        }
                    }
                }
            }
            else
            {
                // Si no es modificable, el costo es el que ya tiene la entidad en la BD
                costoTotalReal = promocion.CalcularCostoTotal();
            }

            // Comparamos y agregamos el error si es necesario
            if (viewModel.Precio < costoTotalReal)
            {
                ModelState.AddModelError("Precio", $"El precio no puede ser menor que el costo total (${costoTotalReal:N2}).");
            }

            // Si agregamos nuestro error personalizado, volvemos a la vista
            if (!ModelState.IsValid)
            {
                RepoblarViewModelParaModificar(viewModel);
                return View(viewModel);
            }
            // --- FIN DE LA VALIDACIÓN MANUAL ---

            // El resto de la lógica de actualización no cambia
            if (esModificable)
            {
                promocion.ActualizarDesdeViewModel(viewModel);
            }
            else
            {
                promocion.ActualizarDatosGenerales(viewModel);
            }

            _promocionesRepo.Actualizar(promocion);

            TempData["SuccessMessage"] = "Promoción modificada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar la promoción con ID {PromocionId}", viewModel.IdPromocion);
            TempData["ErrorMessage"] = "Ocurrió un error al modificar la promoción.";
            RepoblarViewModelParaModificar(viewModel);
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
                TempData["ErrorMessage"] = "No existe promocion con este id.";
                return RedirectToAction(nameof(Index));
            }
            if (!_promocionesRepo.PuedeSerEliminada(id))
            {
                _logger.LogWarning("Intento de eliminación de promoción en uso con ID {PromocionId}", id);
                TempData["ErrorMessage"] = "No se puede eliminar la promoción porque ya ha sido registrada en una o más ventas. Prueba con desactivarla colocandole la fecha de hoy como fin.";
                return RedirectToAction(nameof(Index));
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
    public IActionResult _BuscarPromociones(string busqueda, bool inactivos)
    {
        try
        {
            var promocionesVM = _promocionesRepo.ObtenerListadoPromociones();
            if (!inactivos)
            {
                promocionesVM = promocionesVM.Where(p => p.Activa);
            }
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
                .Select(p => new
                {
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

    private void RepoblarViewModelParaModificar(ModificarPromocionVM viewModel)
    {
        // Repoblamos los nombres de los productos para que Select2 los muestre al recargar la página.
        if (viewModel.DetallesPromocion != null)
        {
            foreach (var detalle in viewModel.DetallesPromocion.Where(d => d.IdProducto > 0))
            {
                var producto = _productosRepo.ObtenerPorId(detalle.IdProducto);
                if (producto != null)
                {
                    detalle.IdProducto = producto.IdProducto;
                    detalle.NombreProducto = producto.Producto;
                    detalle.Costo = producto.Costo;
                }
            }
        }
    }

}