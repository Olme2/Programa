using Microsoft.AspNetCore.Mvc;
using ProductosVM;
using System.Text;
using TempDataExtension;
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
        var viewModel = TempData.Get<IndexProductosVM>("FiltrosProductos");

        if (viewModel == null)
        {
            viewModel = new IndexProductosVM();
        }

        TempData.Set("FiltrosProductos", viewModel);

        // La lista inicial de productos estará vacía. AJAX la llenará.
        viewModel.Productos = new List<ListarProductosVM>();

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult FiltrarProductos(IndexProductosVM viewModel)
    {
        // Guardamos el estado actual de los filtros
        TempData.Set("FiltrosProductos", viewModel);

        // Usamos la misma lógica de filtrado que ya tenías
        var productos = _productosRepo.ObtenerListadoProductos(viewModel);
        ViewData["BusquedaActual"] = viewModel.Busqueda;
        // La vista parcial espera una lista de ListarProductosVM
        return PartialView("_ProductosTabla", productos);
    }

    [HttpGet]
    public IActionResult ResetearFiltros()
    {
        TempData.Remove("FiltrosProductos");
        return RedirectToAction("Index");
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
    public IActionResult GenerarListaDePreciosTexto()
    {
        try
        {
            var productos = _productosRepo.ObtenerProductosActivosParaLista();
            var promociones = _promocionesRepo.ObtenerPromocionesActivasParaLista();
            var sb = new StringBuilder();
            sb.AppendLine("*PRODUCTOS*");
            foreach (var producto in productos)
                sb.AppendLine($"{producto.Nombre} | ${producto.Precio.ToString("N2", CG.CulturaES)}");
            sb.AppendLine();
            sb.AppendLine("*PROMOCIONES*");
            foreach (var promo in promociones)
                sb.AppendLine($"{promo.Nombre} | ${promo.Precio.ToString("N2", CG.CulturaES)}");
            return Json(new { success = true, listaDePrecios = sb.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar la lista de precios.");
            return Json(new { success = false, message = "Error al generar la lista." });
        }
    }

    // GET: Productos/Estadisticas/5
    [HttpGet]
    public IActionResult Estadisticas(int id, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            var inicio = fechaInicio ?? DateTime.Today.AddMonths(-1);
            var fin    = fechaFin    ?? DateTime.Today;
            var vm = _productosRepo.ObtenerEstadisticas(id, inicio, fin);
            if (vm.IdProducto == 0)
            {
                TempData["ErrorMessage"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener estadísticas del producto {Id}", id);
            TempData["ErrorMessage"] = "Error al cargar las estadísticas.";
            return RedirectToAction(nameof(Index));
        }
    }
}
