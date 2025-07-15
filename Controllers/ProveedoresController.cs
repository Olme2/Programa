using Microsoft.AspNetCore.Mvc;

namespace Controllers;

public class ProveedoresController : Controller
{
    private readonly ILogger<ProveedoresController> _logger;
    private IProveedoresRepository repositorioProveedores;

    public ProveedoresController(ILogger<ProveedoresController> logger, IProveedoresRepository RepositorioProveedores)
    {
        _logger = logger;
        repositorioProveedores = RepositorioProveedores;
    }

    public IActionResult Index()
    {
        try
        {
            var proveedoresVM = new List<ListarProveedoresViewModel>();
            var proveedores = repositorioProveedores.ListarProveedores();
            proveedoresVM = proveedores.Select(p => new ListarProveedoresViewModel(p)).ToList();
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
    public IActionResult AltaProveedor(AltaProveedorViewModel proveedorVM)
    {
        try
        {
            if (ModelState.IsValid)
            {
                Proveedores proveedor = new Proveedores(proveedorVM);
                repositorioProveedores.CrearNuevoProveedor(proveedor);
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
            Proveedores proveedor = repositorioProveedores.ObtenerDetallesDeProveedorPorId(id);
            ModificarProveedorViewModel proveedorVM = new ModificarProveedorViewModel(proveedor);
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
    public IActionResult ModificarProveedor(ModificarProveedorViewModel proveedorVM)
    {
        try
        {
            if (ModelState.IsValid)
            {
                Proveedores proveedor = new Proveedores(proveedorVM);
                repositorioProveedores.ModificarProveedor(proveedor);
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
            var proveedor = repositorioProveedores.ObtenerDetallesDeProveedorPorId(id);
            var proveedorVM = new ListarProveedoresViewModel(proveedor);
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
    public IActionResult EliminarProveedor(ListarProveedoresViewModel proveedorVM)
    {
        try
        {
            repositorioProveedores.EliminarProveedorPorId(proveedorVM.IdProveedor);
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