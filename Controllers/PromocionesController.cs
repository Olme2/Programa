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
            TempData["ErrorMessage"] = "Ocurrió un error al cargar las promociones.";
            return View(new List<ListarPromocionesVM>());
        }
    }

    // 1. ACCIÓN GET PARA MOSTRAR EL FORMULARIO
    [HttpGet]
    public IActionResult Alta()
    {
        try
        {
            // Pasamos la lista de todos los productos a la vista.
            // El ViewModel los necesita para el primer selector de productos.
            var productosDisponibles = _productosRepo.ObtenerListadoProductos()
                                                     .Where(p => p.Activo)
                                                     .ToList();

            var viewModel = new AltaPromocionVM(productosDisponibles);

            // Agregamos un primer detalle vacío por defecto, como solicitaste.
            viewModel.DetallesPromocion.Add(new AltaDetallePromocionVM());

            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al cargar el formulario de alta de promoción.");
            TempData["ErrorMessage"] = "Ocurrió un error al preparar el formulario.";
            return RedirectToAction(nameof(Index));
        }
    }

    // 2. ACCIÓN POST PARA RECIBIR LOS DATOS
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaPromocionVM viewModel)
    {
        // Re-validamos que el costo no supere el precio en el servidor.
        // (Este es un ejemplo de validación de servidor que veremos en el "Consejo de Mentor")

        if (!ModelState.IsValid)
        {
            // Si el modelo no es válido, volvemos a cargar la lista de productos y devolvemos la vista.
            var productosDisponibles = _productosRepo.ObtenerListadoProductos().Where(p => p.Activo).ToList();
            viewModel.Productos = productosDisponibles;
            return View(viewModel);
        }

        try
        {
            var nuevaPromocion = Promociones.CrearDesdeViewModel(viewModel);
            _promocionesRepo.Crear(nuevaPromocion);

            TempData["SuccessMessage"] = "¡Promoción creada exitosamente!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al guardar la nueva promoción.");
            TempData["ErrorMessage"] = "Ocurrió un error al guardar la promoción.";
            var productosDisponibles = _productosRepo.ObtenerListadoProductos().Where(p => p.Activo).ToList();
            viewModel.Productos = productosDisponibles;
            return View(viewModel);
        }
    }

    // 3. NUEVO ENDPOINT PARA BÚSQUEDA AJAX DE PRODUCTOS
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
           
           // ¡La lógica clave de exclusión!
           if (excluir != null && excluir.Length > 0)
           {
               query = query.Where(p => !excluir.Contains(p.IdProducto));
           }

           var productos = query
               .OrderBy(p => p.Producto)
               // Devolvemos datos extra (precio, costo) que nuestro JavaScript usará.
               .Select(p => new { 
                   id = p.IdProducto, 
                   text = p.Producto,
                   costo = p.Costo,
                   precio = p.Precio
               })
               .Take(10) // Limitamos a 10 resultados para no sobrecargar
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
        // Pasamos el índice a la vista parcial para que genere los nombres de input correctos
        ViewData["index"] = index;
        return PartialView("Views/Shared/_DetallePromocionItem.cshtml", new DetallesPromocionesVM.AltaDetallePromocionVM());
    }
    // --- ACCIONES PARA MODIFICAR PROMOCIÓN ---

    // GET: /Promociones/Modificar
    // Muestra el formulario para editar una promoción existente.
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

            // ¡EL PASO CLAVE! Cargamos la lista de todos los productos disponibles.
            viewModel.Productos = _productosRepo.ObtenerListadoProductos().ToList();

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
        if (!ModelState.IsValid)
        {
            // Si la validación falla, DEBEMOS recargar la lista de productos antes de devolver la vista.
            viewModel.Productos = _productosRepo.ObtenerListadoProductos().ToList();
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

            TempData["SuccessMessage"] = "Promoción modificada exitosamente!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar la promoción con ID {PromocionId}", viewModel.IdPromocion);
            ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los cambios.");

            // También aquí recargamos los productos antes de mostrar el error.
            viewModel.Productos = _productosRepo.ObtenerListadoProductos().ToList();
            return View(viewModel);
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
    
    [HttpGet]
    public IActionResult _BuscarPromociones(string busqueda)
    {
        try
        {
            IEnumerable<ListarPromocionesVM> promocionesVM = _promocionesRepo.ObtenerListadoPromociones();

            if (!string.IsNullOrEmpty(busqueda))
            {
                promocionesVM = promocionesVM.Where(p =>
                    p.Promocion.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase) ||
                    p.ProductosConcatenados.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase)
                );
            }

            var promocionesOrdenadas = promocionesVM
                .OrderByDescending(p => p.Activa)
                .ThenByDescending(p => p.Inicio);

            // Pasamos el término de búsqueda a la vista parcial para que sepa qué resaltar.
            ViewData["BusquedaActual"] = busqueda;

            return PartialView("_PromocionesTabla", promocionesOrdenadas.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en la búsqueda dinámica de promociones.");
            return StatusCode(500); // Devuelve un error interno del servidor
        }
    }
}