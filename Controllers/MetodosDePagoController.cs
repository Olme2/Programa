using MetodosVM;
using Microsoft.AspNetCore.Mvc;
public class MetodosDePagoController : Controller
{
    private readonly ILogger<ProductosController> _logger;
    private readonly IMetodosPagoRepository _metodosDePagoRepo;
    public MetodosDePagoController(ILogger<ProductosController> logger, IMetodosPagoRepository metodosDePagoRepository)
    {
        _logger = logger;
        _metodosDePagoRepo = metodosDePagoRepository;
    }
    public IActionResult Index()
    {
        try
        {
            var metodosDePago = _metodosDePagoRepo.ListarMetodosDePagoRegistrados();
            var metodosDePagoVM = metodosDePago.Select(m => new ListarMetodosDePagoVM(m)).ToList();
            return View(metodosDePagoVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar la lista de metodos de pago correctamente.";
            return RedirectToAction("Index", "Home");
        }
    }
    
    [HttpGet]
    public IActionResult AltaMetodoDePago()
    {
        return View(new AltaMetodoDePagoVM());
    }

    [HttpPost]
    public IActionResult AltaMetodoDePago(AltaMetodoDePagoVM vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var metodo = new MetodosDePago(vm);
            _metodosDePagoRepo.CrearMetodoDePago(metodo);
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo crear el método de pago.";
            return View(vm);
        }
    }

    [HttpGet]
    public IActionResult ModificarMetodoDePago(short id)
    {
        try
        {
            var metodo = _metodosDePagoRepo.ObtenerDetallesDeMetodoDePagoPorId(id);
            var vm = new ModificarMetodoDePagoVM(metodo);
            return View(vm);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el método de pago para modificar.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult ModificarMetodoDePago(ModificarMetodoDePagoVM vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            var metodo = new MetodosDePago(vm);
            _metodosDePagoRepo.ModificarMetodoDePago(metodo);
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo modificar el método de pago.";
            return View(vm);
        }
    }

    [HttpGet]
    public IActionResult EliminarMetodoDePago(short id)
    {
        try
        {
            var metodo = _metodosDePagoRepo.ObtenerDetallesDeMetodoDePagoPorId(id);
            var vm = new ModificarMetodoDePagoVM(metodo);
            return View(vm);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el método de pago para eliminar.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult EliminarMetodoDePago(ListarMetodosDePagoVM metodoDePagoVM)
    {
        try
        {
            _metodosDePagoRepo.EliminarMetodoDePagoPorId(metodoDePagoVM.IdMetodo);
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo eliminar el método de pago.";
            return RedirectToAction("Index");
        }
    }

}