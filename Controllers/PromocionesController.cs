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
    public IActionResult Index(string busqueda)
    {
        try
        {
            // 1. Llamamos al nuevo método que nos devuelve los VMs listos.
            IEnumerable<ListarPromocionesVM> promocionesVM = _promocionesRepo.ObtenerListadoPromociones();

            // 2. Filtramos por nombre de promoción o de producto.
            if (!string.IsNullOrEmpty(busqueda))
            {
                promocionesVM = promocionesVM.Where(p =>
                    p.Promocion.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase) ||
                    p.ProductosConcatenados.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase)
                );
            }

            // 3. Aplicamos el ordenamiento requerido.
            var promocionesOrdenadas = promocionesVM
                .OrderByDescending(p => p.Activa) // true (activas) primero
                .ThenByDescending(p => p.Inicio);  // Luego, las más recientes primero

            ViewData["BusquedaActual"] = busqueda;

            return View(promocionesOrdenadas.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el listado de promociones.");
            ViewBag.ErrorMessage = "Ocurrió un error al cargar las promociones.";
            return View(new List<ListarPromocionesVM>());
        }
    }

    [HttpGet]
    public IActionResult Alta()
    {
        try
        {
            // 1. Obtenemos los ViewModels de productos LISTOS desde el repositorio.
            var productosVM = _productosRepo.ObtenerListadoProductos().ToList();

            // 2. Creamos el ViewModel principal.
            var viewModel = new AltaPromocionVM(productosVM);

            // 3. Añadimos el primer detalle vacío para la vista.
            viewModel.DetallesPromocion.Add(new AltaDetallePromocionVM());

            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al preparar el formulario de Alta de Promoción.");
            TempData["ErrorMessage"] = "Ocurrió un error al cargar el formulario.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Promociones/Alta
    // Procesa los datos del formulario al guardar.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaPromocionVM promocionVM)
    {
        try
        {
            // ... (Validación de productos duplicados, como ya la teníamos) ...

            if (!ModelState.IsValid)
            {
                // Si la validación falla, recargamos la lista de productos
                // usando el método eficiente.
                promocionVM.Productos = _productosRepo.ObtenerListadoProductos().ToList();
                return View(promocionVM);
            }

            var nuevaPromocion = Promociones.CrearDesdeViewModel(promocionVM);
            _promocionesRepo.Crear(nuevaPromocion);

            TempData["SuccessMessage"] = "Promoción creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear la nueva promoción.");
            ViewBag.ErrorMessage = "Ocurrió un error al guardar la promoción.";

            // Si hay un error, también recargamos usando el método eficiente.
            promocionVM.Productos = _productosRepo.ObtenerListadoProductos().ToList();
            return View(promocionVM);
        }
    }

    // --- ENDPOINT PARA FUNCIONALIDAD DINÁMICA (AJAX) ---

    // GET: /Promociones/ObtenerProducto/{id}
    // Devuelve los datos de un producto en formato JSON para que JavaScript los use.
    [HttpGet]
    public IActionResult ObtenerProducto(int id)
    {
        try
        {
            var producto = _productosRepo.ObtenerPorId(id);
            if (producto == null)
            {
                return NotFound(new { message = "Producto no encontrado." });
            }
            
            return Json(new { costo = producto.Costo, precio = producto.Precio });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener datos del producto con ID {ProductoId}", id);
            return StatusCode(500, new { message = "Ocurrió un error en el servidor." });
        }
    }

    
    // --- ACCIONES PARA MODIFICAR PROMOCIÓN ---

    // GET: /Promociones/Modificar
    // Muestra el formulario para editar una promoción existente.
    [HttpGet]
    public IActionResult Modificar(int id)
    {
        try
        {
            // 1. Obtenemos la promoción a modificar, con todos sus datos.
            var promocion = _promocionesRepo.ObtenerPorId(id);
            if (promocion == null)
            {
                _logger.LogWarning("Se intentó modificar una promoción inexistente con ID {PromocionId}", id);
                return NotFound();
            }

            // 2. Obtenemos la lista de productos para los menús desplegables.
            var productosVM = _productosRepo.ObtenerListadoProductos().ToList();

            // 3. Usamos el constructor del ViewModel que mapea la entidad y la lista de productos.
            var viewModel = new ModificarPromocionVM(promocion, productosVM);

            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al preparar el formulario de modificación para la promoción con ID {PromocionId}", id);
            TempData["ErrorMessage"] = "Ocurrió un error al cargar la promoción.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Promociones/Modificar
    // Procesa los datos del formulario de modificación.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(ModificarPromocionVM promocionVM)
    {
        try
        {
            // Validación de seguridad para productos duplicados.
            if (promocionVM.DetallesPromocion != null)
            {
                var productosIds = promocionVM.DetallesPromocion.Select(d => d.IdProducto);
                if (productosIds.Count() != productosIds.Distinct().Count())
                {
                    ModelState.AddModelError("DetallesPromocion", "No se puede seleccionar el mismo producto más de una vez.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Si la validación falla, recargamos la lista de productos para la vista.
                promocionVM.Productos = _productosRepo.ObtenerListadoProductos().ToList();
                return View(promocionVM);
            }

            // Aplicamos el patrón "Obtener, Actualizar, Guardar".
            var promocionExistente = _promocionesRepo.ObtenerPorId(promocionVM.IdPromocion);
            if (promocionExistente == null)
            {
                return NotFound();
            }

            // Usamos el método de nuestro modelo de dominio para aplicar los cambios.
            promocionExistente.ActualizarDesdeViewModel(promocionVM);

            _promocionesRepo.Actualizar(promocionExistente);

            TempData["SuccessMessage"] = "Promoción modificada correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar la promoción con ID {PromocionId}", promocionVM.IdPromocion);
            ViewBag.ErrorMessage = "Ocurrió un error al guardar los cambios.";

            promocionVM.Productos = _productosRepo.ObtenerListadoProductos().ToList();
            return View(promocionVM);
        }
    }

    // --- ACCIÓN PARA VER DETALLES ---

    // GET: /Promociones/VerDetalles/5
    [HttpGet]
    public IActionResult VerDetalles(int id)
    {
        try
        {
            // 1. Obtenemos la promoción completa con todos sus detalles y productos relacionados.
            var promocion = _promocionesRepo.ObtenerPorId(id);
            if (promocion == null)
            {
                _logger.LogWarning("Se intentó ver los detalles de una promoción inexistente con ID {PromocionId}", id);
                return NotFound();
            }
    
            // 2. Verificamos si la promoción puede ser eliminada.
            bool esEliminable = _promocionesRepo.PuedeSerEliminada(id);
    
            // 3. Mapeamos la entidad de dominio a nuestro nuevo ViewModel específico para esta vista.
            var viewModel = new VerDetallesPromocionVM(promocion, esEliminable);
    
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al cargar los detalles de la promoción con ID {PromocionId}", id);
            TempData["ErrorMessage"] = "Ocurrió un error al cargar los detalles de la promoción.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult Eliminar(int id)
    {
        try
        {
            // Usamos el método de listado y filtramos para obtener el VM ya listo.
            var promocionVM = _promocionesRepo.ObtenerListadoPromociones()
                                            .FirstOrDefault(p => p.IdPromocion == id);
            if (promocionVM == null)
            {
                return NotFound();
            }
            return View(promocionVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al cargar la promoción con ID {PromocionId} para eliminar.", id);
            TempData["ErrorMessage"] = "Error al cargar la promoción para eliminar.";
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
                _logger.LogWarning("Intento de eliminación de promoción en uso con ID {PromocionId}", id);
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
}