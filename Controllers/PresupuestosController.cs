using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using entornoPolleria.ViewModels;
namespace Controllers;
public class PresupuestosController : Controller{
    private readonly ILogger<PresupuestosController> _logger;
    private IPresupuestosRepository repositorioPresupuestos;
    private IProductosRepository repositorioProductos;
    public PresupuestosController(ILogger<PresupuestosController> logger, IPresupuestosRepository RepositorioPresupuestos, IProductosRepository RepositorioProductos){
        _logger=logger;
        repositorioPresupuestos = RepositorioPresupuestos;
        repositorioProductos = RepositorioProductos;
    }
    public IActionResult Index(){
        try{
            return View();
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage("No se cargó el listado de presupuestos correctamente");
            return RedirectToAction("Index", "Home");
        }
    }
    [HttpGet]
    public IActionResult AltaPresupuesto(){
        try{
            return View();
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se cargó el formulario de creación de presupuesto correctamente";
            return RedirectToAction("Index");
        }
    }
    [HttpPost]
    public IActionResult AltaPresupuesto(AltaPresupuestoViewModel presupuestoVM){
        try{
            if(ModelState.IsValid){
                Presupuestos presupuesto = new Presupuestos(presupuestoVM);
                repositorioPresupuestos.CrearPresupuesto(presupuesto);
                return RedirectToAction("Index");
            }
            return View(presupuestoVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo crear el presupuesto";
            return View(presupuestoVM);
        }
    }
    [HttpGet]
    public IActionResult MostrarPresupuesto(int id){
        try{
            Presupuestos presupuesto = repositorioPresupuestos.ObtenerPresupuestoPorId(id);
            return View(presupuesto);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el presupuesto";
            return RedirectToAction("Index");
        }
    }
    [HttpGet]
    public IActionResult AgregarProductosAPresupuesto(int id){
        try{
            ViewData["Productos"] = repositorioProductos.ListarProductosRegistrados();
            var presupuestoVM = new AgregarProductosAPresupuestoViewModel();
            presupuestoVM.IdPresupuesto = id;
            return View(presupuestoVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se cargó el formulario de agregar productos correctamente";
            return RedirectToAction("Index");
        }
    }
    [HttpPost]
    public IActionResult AgregarProductosAPresupuesto(AgregarProductosAPresupuestoViewModel presupuestoVM){
        try{
            if(ModelState.IsValid){
                repositorioPresupuestos.AgregarProducto(presupuestoVM.IdPresupuesto, presupuestoVM.IdProducto, presupuestoVM.Cantidad);
                return RedirectToAction("MostrarPresupuesto", new { id = presupuestoVM.IdPresupuesto });
            }
            ViewData["Productos"] = repositorioProductos.ListarProductosRegistrados();
            return View(presupuestoVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo agregar el producto al presupuesto";
            ViewData["Productos"] = repositorioProductos.ListarProductosRegistrados();
            return View(presupuestoVM);
        }
    }
    [HttpGet]
    public IActionResult EliminarProductosDePresupuesto(int id){
        try{
            List<PresupuestosDetalle> detalles = repositorioPresupuestos.MostrarDetallePorId(id);
            EliminarProductosDePresupuestoViewModel presupuestoVM = new EliminarProductosDePresupuestoViewModel();
            presupuestoVM.IdPresupuesto = id;
            presupuestoVM.Detalles = detalles;
            return View(presupuestoVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se cargó la lista de productos del presupuesto correctamente";
            return RedirectToAction("Index");
        }
    }
    [HttpPost]
    public IActionResult EliminarProductosDePresupuesto(EliminarProductosDePresupuestoViewModel presupuestoVM){
        try{
            if(ModelState.IsValid){
                repositorioPresupuestos.EliminarProducto(presupuestoVM.IdPresupuesto, presupuestoVM.IdProducto, presupuestoVM.CantidadVieja, presupuestoVM.CantidadNueva);
                return RedirectToAction("MostrarPresupuesto", new { id = presupuestoVM.IdPresupuesto });
            }
            presupuestoVM.Detalles = repositorioPresupuestos.MostrarDetallePorId(presupuestoVM.IdPresupuesto);
            return View(presupuestoVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo modificar la cantidad del producto";
            presupuestoVM.Detalles = repositorioPresupuestos.MostrarDetallePorId(presupuestoVM.IdPresupuesto);
            return View(presupuestoVM);
        }
    }
    [HttpGet]
    public IActionResult ModificarPresupuesto(int id){
        try{
            Presupuestos presupuesto = repositorioPresupuestos.ObtenerPresupuestoPorId(id);
            ModificarPresupuestoViewModel presupuestoVM = new ModificarPresupuestoViewModel(presupuesto);
            return View(presupuestoVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el presupuesto";
            return RedirectToAction("Index");
        }
    }
    [HttpPost]
    public IActionResult ModificarPresupuesto(ModificarPresupuestoViewModel presupuestoVM){
        try{
            if(ModelState.IsValid){
                Presupuestos presupuesto = new Presupuestos(presupuestoVM);
                repositorioPresupuestos.ModificarPresupuesto(presupuesto);
                return RedirectToAction("Index");
            }
            return View(presupuestoVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo modificar el presupuesto";
            return View(presupuestoVM);
        }
    }
    [HttpGet]
    public IActionResult EliminarPresupuesto(int id){
        try{
            Presupuestos presupuesto = repositorioPresupuestos.ObtenerPresupuestoPorId(id);
            return View(presupuesto);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el presupuesto";
            return RedirectToAction("Index");
        }
    }
    [HttpPost]
    public IActionResult EliminarPresupuesto(Presupuestos presupuesto){
        try{
            repositorioPresupuestos.EliminarPresupuestoPorId(presupuesto.IdPresupuesto);
            return RedirectToAction("Index");
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo eliminar el presupuesto";
            return View(presupuesto);
        }
    }
}