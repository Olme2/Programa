using Microsoft.AspNetCore.Mvc;
using ProveedoresVM;

public class ProveedoresController : Controller
{
    private readonly ILogger<ProveedoresController> _logger;
    private IProveedoresRepository _proveedoresRepo;

    public ProveedoresController(ILogger<ProveedoresController> logger, IProveedoresRepository proveedoresRepo)
    {
        _logger = logger;
        _proveedoresRepo = proveedoresRepo;
    }

    public IActionResult Index()
    {
        try
        {
            var proveedoresVM = new List<ListarProveedoresVM>();
            var proveedores = _proveedoresRepo.ListarProveedores();
            proveedoresVM = proveedores.Select(p => new ListarProveedoresVM(p)).ToList();
            return View(proveedoresVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se cargó la lista de proveedores: " + e.Message;
            if (e.InnerException != null)
                ViewBag.ErrorMessage += " | Detalle: " + e.InnerException.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult AltaProveedor()
    {
        try
        {
            return View();
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el formulario de creación de proveedor correctamente";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult AltaProveedor(AltaProveedorVM proveedorVM)
    {
        try
        {
            if (ModelState.IsValid)
            {
                Proveedores proveedor = new Proveedores(proveedorVM);
                _proveedoresRepo.CrearNuevoProveedor(proveedor);
                return RedirectToAction("Index");
            }
            return View(proveedorVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo crear el proveedor";
            return View(proveedorVM);
        }
    }

    [HttpGet]
    public IActionResult ModificarProveedor(int id)
    {
        try
        {
            Proveedores proveedor = _proveedoresRepo.ObtenerDetallesDeProveedorPorId(id);
            ModificarProveedorVM proveedorVM = new ModificarProveedorVM(proveedor);
            return View(proveedorVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el proveedor";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult ModificarProveedor(ModificarProveedorVM proveedorVM)
    {
        try
        {
            if (ModelState.IsValid)
            {
                Proveedores proveedor = new Proveedores(proveedorVM);
                _proveedoresRepo.ModificarProveedor(proveedor);
                return RedirectToAction("Index");
            }
            return View(proveedorVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo modificar el proveedor";
            return View(proveedorVM);
        }
    }

    [HttpGet]
    public IActionResult EliminarProveedor(int id)
    {
        try
        {
            var proveedor = _proveedoresRepo.ObtenerDetallesDeProveedorPorId(id);
            var proveedorVM = new ListarProveedoresVM(proveedor);
            return View(proveedorVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el proveedor";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult EliminarProveedor(ListarProveedoresVM proveedorVM)
    {
        try
        {
            _proveedoresRepo.EliminarProveedorPorId(proveedorVM.IdProveedor);
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo eliminar el proveedor: " + e.Message;
            if (e.InnerException != null)
                ViewBag.ErrorMessage += " | Detalle: " + e.InnerException.Message;
            return View(proveedorVM);
        }
    }
} 