using Microsoft.AspNetCore.Mvc;
using ProductosVM;
using ProveedoresVM;

public class ProductosController : Controller
{
    private readonly ILogger<ProductosController> _logger;
    private readonly IProductosRepository _productosRepo;
    private readonly IProveedoresRepository _proveedoresRepo;

    public ProductosController(ILogger<ProductosController> logger, IProductosRepository productosRepo, IProveedoresRepository proveedoresRepo)
    {
        _logger = logger;
        _productosRepo = productosRepo;
        _proveedoresRepo = proveedoresRepo;
    }

    [HttpGet]
    public IActionResult Index(string busqueda, string ordenarPor = "alfabetico")
    {
        try
        {
            // 1. Llamamos al nuevo método del repositorio. ¡Obtenemos los VMs directamente!
            IEnumerable<ListarProductosVM> productosVM = _productosRepo.ObtenerListadoProductos();

            // 2. Aplicamos el filtro de búsqueda (ahora sobre el ViewModel).
            if (!string.IsNullOrEmpty(busqueda))
            {
                productosVM = productosVM.Where(p => 
                    p.Producto.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase) || 
                    p.Proveedor.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase)
                );
            }

            // 3. Aplicamos el ordenamiento según el parámetro.
            switch (ordenarPor)
            {
                case "stock":
                    productosVM = productosVM.OrderBy(p => p.Stock);
                    break;
                case "vendidos":
                    productosVM = productosVM.OrderByDescending(p => p.VendidosSemana);
                    break;
                default: // "alfabetico" y cualquier otro valor
                    productosVM = productosVM.OrderBy(p => p.Producto);
                    break;
            }

            // 4. Pasamos los filtros y ordenamiento a la vista.
            ViewData["BusquedaActual"] = busqueda;
            ViewData["OrdenActual"] = ordenarPor;
            return View(productosVM.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el listado de productos.");
            ViewBag.ErrorMessage = "Ocurrió un error al cargar los productos.";
            return View(new List<ListarProductosVM>());
        }
    }

    [HttpGet]
    public IActionResult Alta()
    {
        try
        {
            var proveedoresVM = _proveedoresRepo.ObtenerListadoProveedores().ToList();
            var viewModel = new AltaProductoVM(proveedoresVM);
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al preparar el formulario de alta de producto.");
            // Usamos TempData para que el mensaje sobreviva la redirección al Index
            TempData["ErrorMessage"] = "Ocurrió un error al cargar el formulario.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaProductoVM productoVM)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, debemos recargar los datos para la vista.
                productoVM.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
                return View(productoVM);
            }

            var nuevoProducto = Productos.CrearDesdeViewModel(productoVM);
            _productosRepo.Crear(nuevoProducto);

            // Añadimos el mensaje de éxito para una experiencia de usuario consistente.
            TempData["SuccessMessage"] = "Producto creado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear el nuevo producto.");
            ViewBag.ErrorMessage = "Ocurrió un error al guardar el producto.";

            // Si hay un error, también debemos recargar los datos para la vista.
            productoVM.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
            return View(productoVM);
        }
    }

    // --- ACCIONES DE MODIFICACIÓN ---
    [HttpGet]
    public IActionResult Modificar(int id)
    {
        try
        {
            var producto = _productosRepo.ObtenerPorId(id);
            if (producto == null)
            {
                _logger.LogWarning("Se intentó modificar un producto inexistente con ID {ProductoId}", id);
                return NotFound(); // Devuelve una página de error 404.
            }

            var proveedoresVM = _proveedoresRepo.ObtenerListadoProveedores().ToList();
            
            // Usamos el constructor que mapea desde el modelo.
            var productoVM = new ModificarProductoVM(producto, proveedoresVM);
            
            return View(productoVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al cargar el producto con ID {ProductoId} para modificar.", id);
            ViewBag.ErrorMessage = "Ocurrió un error al cargar el producto.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(ModificarProductoVM productoVM)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                productoVM.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
                return View(productoVM);
            }

            var productoExistente = _productosRepo.ObtenerPorId(productoVM.IdProducto);
            if (productoExistente == null)
            {
                return NotFound();
            }

            productoExistente.ActualizarDesdeViewModel(productoVM);
            _productosRepo.Actualizar(productoExistente);

            TempData["SuccessMessage"] = "Producto modificado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar el producto con ID {ProductoId}", productoVM.IdProducto);
            ViewBag.ErrorMessage = "Ocurrió un error al guardar los cambios.";
            productoVM.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
            return View(productoVM);
        }
    }

    // --- ACCIONES DE ELIMINACIÓN ---
    [HttpGet]
    public IActionResult Eliminar(int id)
    {
        try
        {
            var producto = _productosRepo.ObtenerPorId(id);
            if (producto == null)
            {
                return NotFound();
            }
            // Reutilizamos el ListarProductosVM para mostrar los datos de confirmación.
            var productoVM = new ListarProductosVM(producto);
            return View(productoVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al cargar el producto con ID {ProductoId} para eliminar.", id);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarConfirmado(int id) // Parámetro simplificado a 'id'
    {
        try
        {
            if (!_productosRepo.PuedeSerEliminado(id))
            {
                _logger.LogWarning("Intento de eliminación de producto en uso con ID {ProductoId}", id);
                TempData["ErrorMessage"] = "No se puede eliminar el producto porque está siendo utilizado en ventas, compras o promociones.";
                return RedirectToAction(nameof(Index));
            }

            _productosRepo.Eliminar(id);

            TempData["SuccessMessage"] = "Producto eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar el producto con ID {ProductoId}", id);
            TempData["ErrorMessage"] = "Ocurrió un error al eliminar el producto.";
            return RedirectToAction(nameof(Index));
        }
    }
}