using Microsoft.AspNetCore.Mvc;
using VentasVM; // Namespace para los ViewModels de Venta
using VentasPromocionesVM;
using DetallesVentasVM;


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
                    OrdenarPor = filtro.OrdenarPor ?? "fecha"
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
                return PartialView("_VentasTabla", ventas.ToList());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error en la búsqueda dinámica de ventas.");
                return StatusCode(500);
            }
        }
    // GET: /Ventas/Alta
    public IActionResult Alta()
    {
        // Lógica para mostrar el formulario de nueva venta (la completaremos después)
        return View();
    }
    // POST: /Ventas/Alta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaVentaVM viewModel)
    {
        // Lógica para guardar la nueva venta (la completaremos después)
        return RedirectToAction(nameof(Index));
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
