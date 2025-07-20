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
    public IActionResult Index(string busqueda, string filtroDeuda = "todos")
    {
        try
        {
            IEnumerable<ListarProveedoresVM> proveedoresVM = _proveedoresRepo.ObtenerListadoProveedores();

            // Aplicamos filtros...
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

            // Aplicamos ordenamiento...
            IOrderedEnumerable<ListarProveedoresVM> proveedoresOrdenados;
            if (filtroDeuda == "todos")
            {
                proveedoresOrdenados = proveedoresVM
                    .OrderByDescending(p => p.Debo > 0)
                    .ThenBy(p => p.Proveedor);
            }
            else
            {
                proveedoresOrdenados = proveedoresVM.OrderBy(p => p.Proveedor);
            }

            ViewData["BusquedaActual"] = busqueda;
            ViewData["FiltroDeudaActual"] = filtroDeuda;

            // ¡AHORA USAMOS LA VARIABLE CORRECTA!
            return View(proveedoresOrdenados.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el listado de proveedores.");
            ViewData["ErrorMessage"] = "Ocurrió un error al cargar los proveedores.";
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
    public IActionResult Alta(AltaProveedorVM proveedorVM)
    {
        try
        {
            // 1. Validamos que los datos del formulario cumplan las reglas.
            if (!ModelState.IsValid)
            {
                // Si no son válidos, volvemos a mostrar el formulario con los errores.
                return View(proveedorVM);
            }

            // 2. Usamos el Factory Method de nuestro modelo de dominio.
            //    Aquí ocurre la transformación de ViewModel a Modelo.
            var nuevoProveedor = Proveedores.CrearDesdeViewModel(proveedorVM);

            // 3. Usamos el método estandarizado del repositorio para guardar.
            _proveedoresRepo.Crear(nuevoProveedor);

            // 4. Redirigimos al Index (Patrón Post-Redirect-Get).
            TempData["SuccessMessage"] = "Proveedor creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear el nuevo proveedor.");
            ViewBag.ErrorMessage = "Ocurrió un error al guardar el proveedor.";
            return View(proveedorVM);
        }
    }

    // --- ACCIONES DE MODIFICACIÓN ---
    [HttpGet]
    public IActionResult Modificar(int id)
    {
        try
        {
            var proveedor = _proveedoresRepo.ObtenerPorId(id);
            if (proveedor == null)
            {
                _logger.LogWarning("Se intentó modificar un proveedor inexistente con ID {ProveedorId}", id);
                return NotFound();
            }
            var proveedorVM = new ModificarProveedorVM(proveedor);
            return View(proveedorVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al cargar el proveedor con ID {ProveedorId} para modificar.", id);
            TempData["ErrorMessage"] = "Error al cargar el proveedor. Intente de nuevo."; // Usar TempData para errores en redirección
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(ModificarProveedorVM proveedorVM)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(proveedorVM);
            }
            var proveedorExistente = _proveedoresRepo.ObtenerPorId(proveedorVM.IdProveedor);
            if (proveedorExistente == null)
            {
                return NotFound();
            }
            proveedorExistente.ActualizarDesdeViewModel(proveedorVM);
            _proveedoresRepo.Actualizar(proveedorExistente);

            // --- Mensaje de éxito ---
            TempData["SuccessMessage"] = "Proveedor modificado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar el proveedor con ID {ProveedorId}", proveedorVM.IdProveedor);
            ViewBag.ErrorMessage = "Ocurrió un error al guardar los cambios.";
            return View(proveedorVM);
        }
    }

    // --- ACCIONES DE ELIMINACIÓN ---
    [HttpGet]
    public IActionResult Eliminar(int id)
    {
        try
        {
            var proveedorVM = _proveedoresRepo.ObtenerListadoProveedores()
                                            .FirstOrDefault(p => p.IdProveedor == id);
            if (proveedorVM == null)
            {
                return NotFound();
            }
            return View(proveedorVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al cargar el proveedor con ID {ProveedorId} para eliminar.", id);
            TempData["ErrorMessage"] = "Error al cargar el proveedor para eliminar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    // Cambiamos el nombre del parámetro a 'id' para que coincida con la convención de routing
    public IActionResult EliminarConfirmado(int id)
    {
        try
        {
            // ¡VERIFICACIÓN DE SEGURIDAD!
            // Volvemos a consultar al repositorio para estar 100% seguros.
            var proveedorParaEliminar = _proveedoresRepo.ObtenerListadoProveedores()
                                                       .FirstOrDefault(p => p.IdProveedor == id);

            if (proveedorParaEliminar != null && !proveedorParaEliminar.EsEliminable)
            {
                _logger.LogWarning("Intento de eliminación de proveedor en uso con ID {ProveedorId}", id);
                TempData["ErrorMessage"] = "No se puede eliminar el proveedor porque tiene productos asociados.";
                return RedirectToAction(nameof(Index));
            }

            _proveedoresRepo.Eliminar(id);
            TempData["SuccessMessage"] = "Proveedor eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar el proveedor con ID {ProveedorId}", id);
            // Usamos TempData porque estamos redirigiendo. ViewBag se perdería.
            TempData["ErrorMessage"] = "Ocurrió un error al eliminar el proveedor. Es posible que esté asociado a productos existentes.";
            return RedirectToAction(nameof(Index));
        }
    }
    
     // --- NUEVA ACCIÓN PARA BÚSQUEDA DINÁMICA (AJAX) ---
    // GET: /Proveedores/_BuscarProveedores
    [HttpGet]
    public IActionResult _BuscarProveedores(string busqueda, string filtroDeuda = "todos")
    {
        try
        {
            // La lógica de filtrado y ordenamiento es EXACTAMENTE la misma que en el Index.
            IEnumerable<ListarProveedoresVM> proveedoresVM = _proveedoresRepo.ObtenerListadoProveedores();

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

            IOrderedEnumerable<ListarProveedoresVM> proveedoresOrdenados;
            if (filtroDeuda == "todos")
            {
                proveedoresOrdenados = proveedoresVM
                    .OrderByDescending(p => p.Debo > 0)
                    .ThenBy(p => p.Proveedor);
            }
            else
            {
                proveedoresOrdenados = proveedoresVM.OrderBy(p => p.Proveedor);
            }

            // Pasamos el término de búsqueda a la vista para poder resaltar las coincidencias.
            ViewData["BusquedaActual"] = busqueda;

            // En lugar de devolver una Vista completa, devolvemos una Vista Parcial.
            return PartialView("_ProveedoresTabla", proveedoresOrdenados.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en la búsqueda dinámica de proveedores.");
            // En caso de error, devolvemos un código de error para que el JavaScript lo maneje.
            return StatusCode(500);
        }
    }
}