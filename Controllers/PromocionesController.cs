using Microsoft.AspNetCore.Mvc;
using PromocionesVM;
public class PromocionesController : Controller
{
    private readonly ILogger<ProveedoresController> _logger;
    private IPromocionesRepository _promocionesRepo;
    private IProveedoresRepository _proveedoresRepo;
    private IProductosRepository _productosRepo;

    public PromocionesController(ILogger<ProveedoresController> logger, IPromocionesRepository promocionesRepo, IProveedoresRepository proveedoresRepo, IProductosRepository productosRepo)
    {
        _logger = logger;
        _promocionesRepo = promocionesRepo;
        _proveedoresRepo = proveedoresRepo;
        _productosRepo = productosRepo;
    }

    [HttpGet]
    public IActionResult Index()
    {
        try
        {
            var promociones = _promocionesRepo.ListarPromocionesRegistradas();
            var promocionesVM = promociones.Select(p => {
            var productosNombres = new List<string>();
            decimal costoTotal = 0;
            foreach (var detalle in p.DetallesPromocion)
            {
                var producto = _productosRepo.ObtenerDetallesDeProductoPorId(detalle.IdProducto);
                productosNombres.Add(producto.Producto);
                costoTotal += producto.Costo * detalle.Cantidad;
            }
            return new ListarPromocionesVM(p, costoTotal, productosNombres);
            }).ToList();
            return View(promocionesVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar la lista de promociones: " + e.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult ModificarPromocion(int id)
    {
        try
        {
            var promocion = _promocionesRepo.ObtenerDetallesDePromocionPorId(id);
            var costo =
            var vm = new ModificarPromocionVM(promocion);
            return View(vm);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar la promoción";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult ModificarPromocion(PromocionesVM.ModificarPromocionVM promocionVM)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var promocion = new Promociones(promocionVM); // Asegúrate de tener este constructor
                _promocionesRepo.ModificarPromocion(promocion);
                return RedirectToAction("Index");
            }
            return View(promocionVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo modificar la promoción";
            return View(promocionVM);
        }
    }

    [HttpGet]
    public IActionResult EliminarPromocion(int id)
    {
        try
        {
            var promocion = _promocionesRepo.ObtenerDetallesDePromocionPorId(id);
            var vm = new ListarPromocionesVM(promocion, 0, new List<string>()); // Ajustar lógica de costo/productos si es necesario
            return View(vm);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar la promoción";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult EliminarPromocion(ListarPromocionesVM promocionVM)
    {
        try
        {
            _promocionesRepo.EliminarPromocionPorId(promocionVM.IdPromocion);
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo eliminar la promoción: " + e.Message;
            return View(promocionVM);
        }
    }
}