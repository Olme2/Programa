using Microsoft.AspNetCore.Mvc;
using ProductosVM;

public class ProductosController : Controller
{
    private readonly ILogger<ProductosController> _logger;
    private readonly IProductosRepository _productosRepo;
    private readonly IProveedoresRepository _proveedoresRepo;
    private readonly IPromocionesRepository _promocionesRepo;
    public ProductosController(ILogger<ProductosController> logger, IProductosRepository productosRepo, IProveedoresRepository proveedoresRepo, IPromocionesRepository promocionesRepo)
    {
        _logger = logger;
        _productosRepo = productosRepo;
        _proveedoresRepo = proveedoresRepo;
        _promocionesRepo = promocionesRepo;
    }

    [HttpGet]
    public IActionResult Index()
    {
        try
        {
            var productosVM = _productosRepo.ObtenerListadoProductos().Where(p => p.Activo).OrderBy(p => p.Producto).ToList();                
            return View(productosVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el listado de productos.");
            TempData["ErrorMessage"] = "No se pudo cargar el listado de productos.";
            return View(new List<ListarProductosVM>());
        }
    }

    [HttpGet]
    public IActionResult Alta()
    {
        return View(new AltaProductoVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaProductoVM viewModel)
    {
        if (!ModelState.IsValid)
        {
            var proveedor = _proveedoresRepo.ObtenerPorId(viewModel.IdProveedor);
            if(proveedor != null)
            viewModel.Proveedor = proveedor.Proveedor;
            return View(viewModel);
        }

        try
        {
            var producto = Productos.CrearDesdeViewModel(viewModel);
            _productosRepo.Crear(producto);
            TempData["SuccessMessage"] = "Producto creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al crear el producto.");
            TempData["ErrorMessage"] = "Ocurrió un error al crear el producto.";
            return View(viewModel);
        }
    }
    
    [HttpGet]
    public IActionResult Modificar(int id)
    {
        try
        {
            var producto = _productosRepo.ObtenerPorId(id);
            if (producto == null)
            {
                TempData["ErrorMessage"] = "No existe producto con ese id.";
                return RedirectToAction(nameof(Index));
            }
            
            // Pasamos la lista de proveedores para el dropdown.
            var proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
            var viewModel = new ModificarProductoVM(producto, proveedores);
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el producto con ID {ProductoId} para modificar.", id);
            TempData["ErrorMessage"] = "No se pudo cargar el producto para modificar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(ModificarProductoVM viewModel)
    {
        if (!ModelState.IsValid)
        {
            // Si la validación falla, recargamos la lista de proveedores.
            viewModel.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
            return View(viewModel);
        }
        
        try
        {
            var producto = _productosRepo.ObtenerPorId(viewModel.IdProducto);
            if (producto == null)
            {
                TempData["ErrorMessage"] = "No existe producto con ese id.";
                return RedirectToAction(nameof(Index));
            }
            if (producto.Activo && !viewModel.Activo)
            {
                try
                {
                    _promocionesRepo.DesactivarPorIdProducto(producto.IdProducto);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error al modificar la promocion");
                    TempData["ErrorMessage"] = "Ocurrió un error al desactivar la promocion.";
                    viewModel.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
                    return View(viewModel);
                }
            }
            producto.ActualizarDesdeViewModel(viewModel);
            _productosRepo.Actualizar(producto);
            TempData["SuccessMessage"] = "Producto modificado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar el producto con ID {ProductoId}", viewModel.IdProducto);
            TempData["ErrorMessage"] = "Ocurrió un error al modificar el producto.";
            viewModel.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
            return View(viewModel);
        }
    }
    
    [HttpGet]
    public IActionResult Eliminar(int id)
    {
        try
        {
            var productoVM = _productosRepo.ObtenerListadoProductos().FirstOrDefault(p => p.IdProducto == id);
            if (productoVM == null)
            {
                TempData["ErrorMessage"] = "No existe producto con ese id.";
                return RedirectToAction(nameof(Index));
            }
            if (!_productosRepo.PuedeSerEliminado(id))
            {
                TempData["ErrorMessage"] = "No se puede eliminar el producto porque está en uso en promociones, ventas o compras. Prueba desactivandolo.";
                return RedirectToAction(nameof(Index));
            }
            return View(productoVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el producto con ID {ProductoId} para eliminar.", id);
            TempData["ErrorMessage"] = "No se pudo cargar el producto para eliminar.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarConfirmado(ListarProductosVM viewModel)
    {
        try
        {
            _productosRepo.Eliminar(viewModel.IdProducto);
            TempData["SuccessMessage"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar el producto con ID {ProductoId}", viewModel.IdProducto);
            TempData["ErrorMessage"] = "Ocurrió un error al eliminar el producto.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult _BuscarProductos(string? busqueda, string ordenarPor, bool inactivos)
    {
        try
        {
            // 1. Empezamos con la consulta base.
            var query = _productosRepo.ObtenerListadoProductos();

            // 2. Aplicamos el filtro de inactivos.
            // Si el checkbox NO está marcado, filtramos para mostrar solo los activos.
            if (!inactivos)
            {
                query = query.Where(p => p.Activo);
            }

            // 3. Aplicamos el filtro de búsqueda por texto.
            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(p =>
                    p.Producto.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase) ||
                    p.Proveedor.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase)
                );
            }

            // 4. Aplicamos el ordenamiento.
            // Usamos IOrderedEnumerable para poder encadenar el ordenamiento.
            IOrderedEnumerable<ListarProductosVM> productosOrdenados;

            switch (ordenarPor)
            {
                case "stock":
                    // Siempre ordenamos por Activo descendente primero.
                    productosOrdenados = query.OrderByDescending(p => p.Activo).ThenByDescending(p => p.Stock);
                    break;
                default: // "alfabetico"
                    productosOrdenados = query.OrderByDescending(p => p.Activo).ThenBy(p => p.Producto);
                    break;
            }

            ViewData["BusquedaActual"] = busqueda;
            
            // 5. Devolvemos la vista parcial con la lista filtrada y ordenada.
            return PartialView("_ProductosTabla", productosOrdenados.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en la búsqueda dinámica de productos.");
            return StatusCode(500); // Es una buena práctica devolver un código de error para AJAX.
        }
    }
}