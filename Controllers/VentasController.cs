using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VentasVM; // Namespace para los ViewModels de Venta
using VentasPromocionesVM;
using DetallesVentasVM;
using System.Globalization;

public class VentasController : Controller
{
    private readonly IVentaRepository _ventaRepo;
    private readonly IProductosRepository _productosRepo;
    private readonly IPromocionesRepository _promocionesRepo;
    private readonly IMetodosPagoRepository _metodosPagoRepo;
    private readonly ILogger<VentasController> _logger;

    public VentasController(
        IVentaRepository ventaRepo,
        IProductosRepository productosRepo,
        IPromocionesRepository promocionesRepo,
        IMetodosPagoRepository metodosPagoRepo,
        ILogger<VentasController> logger)
    {
        _ventaRepo = ventaRepo;
        _productosRepo = productosRepo;
        _promocionesRepo = promocionesRepo;
        _metodosPagoRepo = metodosPagoRepo;
        _logger = logger;
    }
    // GET: /Ventas
    [HttpGet]
        public IActionResult Index(IndexVentasVM filtro)
        {
            try
            {
                var viewModel = new IndexVentasVM
                {
                    FechaInicio = filtro.FechaInicio == default ? DateTime.Today : filtro.FechaInicio,
                    FechaFin = filtro.FechaFin == default ? DateTime.Today : filtro.FechaFin,
                    Busqueda = filtro.Busqueda,
                    IdMetodoPago = filtro.IdMetodoPago,
                };

                viewModel.Ventas = _ventaRepo.ObtenerListadoVentas(viewModel).ToList();
                viewModel.MetodosPago = _metodosPagoRepo.ObtenerListadoMetodosPago().ToList();

                return View(viewModel);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al obtener el listado de ventas.");
                TempData["ErrorMessage"] = "No se pudo cargar el listado de ventas.";
                return View(new IndexVentasVM());
            }
        }

        [HttpGet]
        public IActionResult _BuscarVentas(IndexVentasVM filtro)
        {
            try
            {
                var ventas = _ventaRepo.ObtenerListadoVentas(filtro);
                ViewData["BusquedaActual"] = filtro.Busqueda;
                return PartialView("_VentasTabla", ventas.ToList());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error en la búsqueda dinámica de ventas.");
                return StatusCode(500);
            }
        }
    // GET: /Ventas/Alta
    [HttpGet]
        public IActionResult Alta()
        {
            try
            {
                var viewModel = new AltaVentaVM();
                viewModel.MetodosDePago = ObtenerListaMetodosDePago();
                return View(viewModel);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al preparar el formulario de alta de venta.");
                TempData["ErrorMessage"] = "Ocurrió un error al preparar el formulario.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Alta(AltaVentaVM viewModel)
        {
            if (!ModelState.IsValid)
            {
                RepoblarViewModelParaVista(viewModel);
                return View(viewModel);
            }

            try
            {
                var nuevaVenta = Ventas.CrearDesdeViewModel(viewModel);
                _ventaRepo.CrearVenta(nuevaVenta);
                
                TempData["SuccessMessage"] = "Venta registrada exitosamente.";
                return RedirectToAction("Index", "Home");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación de negocio al crear venta.");
                ModelState.AddModelError(string.Empty, ex.Message);
                RepoblarViewModelParaVista(viewModel);
                return View(viewModel);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error inesperado al crear la venta.");
                TempData["ErrorMessage"] = "Ocurrió un error inesperado al registrar la venta.";
                RepoblarViewModelParaVista(viewModel);
                return View(viewModel);
            }
        }

    // --- MÉTODOS PRIVADOS DE AYUDA ---

    private void RepoblarViewModelParaVista(AltaVentaVM viewModel)
    {
        viewModel.MetodosDePago = _metodosPagoRepo.ObtenerListadoMetodosPago()
            .Select(m => new SelectListItem { Value = m.IdMetodo.ToString(), Text = m.Metodo })
            .ToList();

        // Repoblamos los nombres de los productos y promociones para que Select2 los muestre.
        if (viewModel.DetallesVenta != null)
        {
            foreach (var detalle in viewModel.DetallesVenta.Where(d => d.IdProducto > 0))
            {
                var producto = _productosRepo.ObtenerPorId(detalle.IdProducto);
                if (producto != null)
                {
                    detalle.IdProducto = producto.IdProducto;
                    detalle.NombreProducto = producto.Producto + " ($" + producto.Precio + ") S: " + producto.Stock.ToString("N3", ConfiguracionGlobal.CulturaES);
                    // Asignamos el precio y costo por si se perdieron
                    detalle.PrecioUnitario = producto.Precio;
                    detalle.CostoUnitario = producto.Costo;
                }
            }
        }
        if (viewModel.VentaPromociones != null)
        {
            foreach (var promo in viewModel.VentaPromociones.Where(p => p.IdPromocion > 0))
            {
                var promocion = _promocionesRepo.ObtenerPorId(promo.IdPromocion);
                if (promocion != null)
                {
                    promo.IdPromocion = promocion.IdPromocion;
                    promo.NombrePromocion = promocion.Promocion;
                    // Asignamos el precio y costo por si se perdieron
                    promo.PrecioPromo = promocion.Precio;
                    promo.CostoPromo = promocion.CalcularCostoTotal();
                }
            }
        }
    }

        
        private List<SelectListItem> ObtenerListaMetodosDePago()
        {
             return _metodosPagoRepo.ObtenerListadoMetodosPago()
                .Select(m => new SelectListItem { Value = m.IdMetodo.ToString(), Text = m.Metodo })
                .ToList();
        }

        // --- ACCIONES AJAX PARA LA VISTA ---
        
        [HttpGet]
        public IActionResult ObtenerVistaDetalleVenta(int index)
        {
            ViewData["index"] = index;
            return PartialView("Partials/_DetalleVentaItem", new AltaDetalleVentaVM());
        }

        [HttpGet]
        public IActionResult ObtenerVistaVentaPromocion(int index)
        {
            ViewData["index"] = index;
            return PartialView("Partials/_VentaPromocionItem", new AltaVentaPromocionVM());
        }

        [HttpGet]
        public IActionResult BuscarProductosParaVenta(string term, [FromQuery] int[] excluir)
        {
            var query = _productosRepo.ObtenerListadoProductos().Where(p => p.Activo && p.Stock > 0);
            if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(p => p.Producto.Contains(term, StringComparison.CurrentCultureIgnoreCase));
            }
            if (excluir != null)
            {
                query = query.Where(p => !excluir.Contains(p.IdProducto));
            }
            var productos = query.OrderByDescending(p => p.VentaSemanal).ThenBy(p => p.Producto)
                .Select(p => new { id = p.IdProducto, text = $"{p.Producto} (${p.Precio}) S: {p.Stock.ToString("N3", ConfiguracionGlobal.CulturaES)}", precio = p.Precio, costo = p.Costo })
                .Take(10).ToList();
            return Json(productos);
        }
        
        [HttpGet]
        public IActionResult BuscarPromocionesParaVenta(string term, [FromQuery] int[] excluir)
        {
            var query = _promocionesRepo.ObtenerListadoPromociones().Where(p => p.Activa && p.Stock > 0);
             if (!string.IsNullOrEmpty(term))
            {
                query = query.Where(p => p.Promocion.Contains(term, StringComparison.CurrentCultureIgnoreCase));
            }
            if (excluir != null)
            {
                query = query.Where(p => !excluir.Contains(p.IdPromocion));
            }
            var promociones = query.OrderByDescending(p => p.VentaSemanal).ThenBy(p => p.Promocion)
                .Select(p => new { id = p.IdPromocion, text = $"{p.Promocion} (${p.Precio}) S: {p.Stock}", precio = p.Precio, costo = p.Costo })
                .Take(10).ToList();
            return Json(promociones);
        }

    // GET: /Ventas/Modificar/5
    public IActionResult Modificar(int id)
    {
        // Lógica para mostrar el formulario de modificación (la completaremos después)
        return View();
    }
    // POST: /Ventas/Modificar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(int id, ModificarVentaVM viewModel)
    {
        // Lógica para guardar los cambios de la venta (la completaremos después)
        return RedirectToAction(nameof(Index));
    }
    // GET: /Ventas/Eliminar/5
    public IActionResult Eliminar(int id)
    {
        // Lógica para mostrar la página de confirmación de eliminación (la completaremos después)
        return View();
    }
    // POST: /Ventas/Eliminar/5
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarConfirmado(int id)
    {
        // Lógica para eliminar la venta (la completaremos después)
        return RedirectToAction(nameof(Index));
    }
}
