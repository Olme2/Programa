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
            // 1. Obtenemos la lista completa de proveedores.
            IEnumerable<Proveedores> proveedores = _proveedoresRepo.ObtenerTodos();

            // 2. Aplicamos el filtro de búsqueda por nombre.
            if (!string.IsNullOrEmpty(busqueda))
            {
                proveedores = proveedores.Where(p => p.Proveedor.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase));
            }

            // 3. Aplicamos el filtro de deuda.
            switch (filtroDeuda)
            {
                case "conDeuda":
                    proveedores = proveedores.Where(p => p.Debo > 0);
                    break;
                case "sinDeuda":
                    proveedores = proveedores.Where(p => p.Debo == 0);
                    break;
                // Si es "todos" o cualquier otro valor, no hacemos nada y mostramos todos.
                default:
                    break;
            }

            // 4. Aplicamos el ordenamiento.
            IOrderedEnumerable<Proveedores> proveedoresOrdenados;
            if (filtroDeuda == "todos")
            {
                // Si mostramos todos, primero los que tienen deuda, luego los que no.
                proveedoresOrdenados = proveedores.OrderByDescending(p => p.Debo > 0).ThenBy(p => p.Proveedor);
            }
            else
            {
                // Para los otros filtros, solo ordenamos alfabéticamente.
                proveedoresOrdenados = proveedores.OrderBy(p => p.Proveedor);
            }

            // 5. Mapeamos a ViewModels.
            var proveedoresVM = proveedoresOrdenados.Select(p => new ListarProveedoresVM(p)).ToList();

            // 6. Pasamos los filtros actuales a la vista para que los controles mantengan su estado.
            ViewData["BusquedaActual"] = busqueda;
            ViewData["FiltroDeudaActual"] = filtroDeuda;
            return View(proveedoresVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el listado de proveedores.");
            ViewBag.ErrorMessage = "Ocurrió un error al cargar los proveedores.";
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
            var proveedor = _proveedoresRepo.ObtenerPorId(id);
            if (proveedor == null)
            {
                return NotFound();
            }
            var proveedorVM = new ListarProveedoresVM(proveedor);
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
            _proveedoresRepo.Eliminar(id);

            // --- Mensaje de éxito ---
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
}