using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ComprasVM;
using DetallesComprasVM;
using TempDataExtension;

public class ComprasController : Controller
{
    private readonly IComprasRepository _comprasRepo;
    private readonly IProveedoresRepository _proveedoresRepo;
    private readonly IProductosRepository _productosRepo;
    private readonly ILogger<ComprasController> _logger;

    public ComprasController(
        IComprasRepository comprasRepo,
        IProveedoresRepository proveedoresRepo,
        IProductosRepository productosRepo,
        ILogger<ComprasController> logger)
    {
        _comprasRepo = comprasRepo;
        _proveedoresRepo = proveedoresRepo;
        _productosRepo = productosRepo;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index(IndexComprasVM filtro)
    {
        try
        {
            var viewModel = TempData.Get<IndexComprasVM>("FiltrosCompras");
            if (viewModel == null)
            {
                viewModel = new IndexComprasVM()
                {
                    IdProveedor = filtro.IdProveedor
                };
            }
            // Poblar siempre el dropdown de proveedores
            viewModel.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
            viewModel.Compras = _comprasRepo.ObtenerListadoCompras(viewModel).ToList();
            TempData.Set("FiltrosCompras", viewModel);
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el listado de compras.");
            TempData["ErrorMessage"] = "No se pudo cargar el listado de compras.";
            return View(new IndexComprasVM());
        }
    }

    [HttpPost]
    public IActionResult _BuscarCompras(IndexComprasVM viewModel)
    {
        try
        {
            TempData.Set("FiltrosCompras", viewModel);
            var compras = _comprasRepo.ObtenerListadoCompras(viewModel).ToList();
            return PartialView("_ComprasTabla", compras);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en la búsqueda dinámica de compras.");
            return StatusCode(500);
        }
    }

    [HttpGet]
    public IActionResult ResetearFiltros()
    {
        TempData.Remove("FiltrosCompras");
        return RedirectToAction("Index");
    }

    // GET: Compras/Alta
    public IActionResult Alta()
    {
        var viewModel = new AltaCompraVM();
        RepoblarAltaCompraViewModel(viewModel);
        return View(viewModel);
    }

    // POST: Compras/Alta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaCompraVM viewModel)
    {
        if (viewModel.DetallesCompra == null || !viewModel.DetallesCompra.Any())
            ModelState.AddModelError(string.Empty, "No hay productos en la compra.");

        if (ModelState.IsValid)
        {
            try
            {
                var nuevaCompra = new Compras(viewModel.IdProveedor, viewModel.Fecha, viewModel.Detalle,
                    viewModel.DetallesCompra.Select(d => new DetallesCompras(d.IdProducto, d.Cantidad, d.CostoUnitario)).ToList());

                _comprasRepo.Crear(nuevaCompra);
                TempData["SuccessMessage"] = "Compra registrada con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la compra.");
                ModelState.AddModelError(string.Empty, "Error al guardar la compra: " + ex.Message);
            }
        }

        RepoblarAltaCompraViewModel(viewModel);
        return View(viewModel);
    }

    // GET: Compras/Modificar/5
    public IActionResult Modificar(int id)
    {
        var compra = _comprasRepo.ObtenerPorId(id);
        if (compra == null)
        {
            TempData["ErrorMessage"] = "La compra no fue encontrada.";
            return RedirectToAction(nameof(Index));
        }

        var proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
        var viewModel = new ModificarCompraVM(compra, proveedores);
        
        return View(viewModel);
    }

    // POST: Compras/Modificar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(int id, ModificarCompraVM viewModel)
    {
        if (id != viewModel.IdCompra)
            return BadRequest();

        if (viewModel.DetallesCompra == null || !viewModel.DetallesCompra.Any())
            ModelState.AddModelError(string.Empty, "La compra debe contener al menos un producto.");

        if (ModelState.IsValid)
        {
            try
            {
                var compraActualizada = new Compras(
                    viewModel.IdProveedor,
                    viewModel.Fecha,
                    viewModel.Detalle,
                    viewModel.DetallesCompra.Select(d => new DetallesCompras(d.IdProducto, d.Cantidad, d.CostoUnitario)).ToList());

                _comprasRepo.Actualizar(id, compraActualizada);
                TempData["SuccessMessage"] = "Compra modificada con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al modificar la compra con ID {Id}", id);
                ModelState.AddModelError(string.Empty, "Error al guardar los cambios: " + ex.Message);
            }
        }

        viewModel.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
        return View(viewModel);
    }
    
    // GET: Compras/Eliminar/5
    public IActionResult Eliminar(int id)
    {
        var compra = _comprasRepo.ObtenerPorId(id);
        if (compra == null)
        {
            TempData["ErrorMessage"] = "La compra no fue encontrada.";
            return RedirectToAction(nameof(Index));
        }
        var viewModel = new ListarComprasVM(compra);
        return View(viewModel);
    }

    // POST: Compras/Eliminar/5
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarConfirmado(int id)
    {
        try
        {
            _comprasRepo.Eliminar(id);
            TempData["SuccessMessage"] = "Compra eliminada con éxito.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la compra con ID {Id}", id);
            TempData["ErrorMessage"] = "Ocurrió un error al eliminar la compra.";
        }
        return RedirectToAction(nameof(Index));
    }


    // --- MÉTODOS PRIVADOS DE AYUDA (HELPERS) ---

    private void RepoblarAltaCompraViewModel(AltaCompraVM viewModel)
    {
        viewModel.Proveedores = _proveedoresRepo.ObtenerListadoProveedores().ToList();
    }

    private Compras MapearAltaViewModelAEntidad(AltaCompraVM viewModel)
    {
        var detalles = viewModel.DetallesCompra.Select(dc => DetallesCompras.CrearDesdeViewModel(dc)).ToList();
        return new Compras(viewModel.IdProveedor, viewModel.Fecha, viewModel.Detalle, detalles);
    }
    
    private Compras MapearModificarViewModelAEntidad(ModificarCompraVM viewModel)
    {
        // Creamos una entidad temporal con los datos actualizados.
        var detallesActualizados = viewModel.DetallesCompra.Select(d =>
        {
            var detalle = new DetallesCompras(d.IdProducto, d.Cantidad, d.CostoUnitario);
            // Si el detalle ya existía, le asignamos su ID original para que el repositorio sepa que debe actualizarlo.
            return detalle;
        }).ToList();

        var compra = new Compras(viewModel.IdProveedor, viewModel.Fecha, viewModel.Detalle, detallesActualizados);
        return compra;
    }


    // --- ENDPOINTS PARA AJAX (Ej: Select2) ---

    [HttpGet]
    public IActionResult ObtenerVistaDetalleCompra()
    {
        return PartialView("Partials/_DetalleCompraItem", new DetalleCompraVM());
    }

    [HttpGet]
    public IActionResult ObtenerVistaModificarDetalleCompra()
    {
        // Retorna una fila nueva (sin producto seleccionado) para agregar en Modificar
        return PartialView("Partials/_ModificarDetalleCompraItem", new DetalleCompraVM());
    }
    
    [HttpGet]
    public JsonResult BuscarProductosParaCompra(string term)
    {
        var productos = _productosRepo.ObtenerListadoProductos(new ProductosVM.IndexProductosVM { Busqueda = term, Inactivos = false });

        var resultadoSelect2 = productos.Select(p => new 
        {
            id = p.IdProducto,
            text = p.Producto,
            costo = p.Costo
        });

        return Json(new { results = resultadoSelect2 });
    }

    // Endpoint para filtrar productos por proveedor (usado en Modificar)
    [HttpGet]
    public JsonResult BuscarProductosPorProveedor(string term, int idProveedor)
    {
        var todos = _productosRepo.ObtenerListadoProductos(new ProductosVM.IndexProductosVM { Busqueda = term, Inactivos = false });

        IEnumerable<ProductosVM.ListarProductosVM> filtrados = todos;
        if (idProveedor > 0)
        {
            // Resolver el nombre del proveedor primero para evitar llamadas dentro de LINQ
            var nombreProveedor = _proveedoresRepo.ObtenerListadoProveedores()
                .FirstOrDefault(pr => pr.IdProveedor == idProveedor)?.Proveedor;
            if (!string.IsNullOrEmpty(nombreProveedor))
                filtrados = todos.Where(p => p.Proveedor == nombreProveedor);
        }

        var resultado = filtrados.Select(p => new
        {
            id = p.IdProducto,
            text = p.Producto,
            costo = p.Costo
        });

        return Json(new { results = resultado });
    }
}