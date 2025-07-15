using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Controllers;

public class ProductosController : Controller{
    private readonly ILogger<ProductosController> _logger;
    private IProductosRepository repositorioProductos;
    private IProveedoresRepository repositorioProveedores;
    public ProductosController(ILogger<ProductosController> logger, IProductosRepository RepositorioProductos, IProveedoresRepository RepositorioProveedores)
    {
        _logger = logger;
        repositorioProductos = RepositorioProductos;
        repositorioProveedores = RepositorioProveedores;
    }
    public IActionResult Index(){
        try{
            var productosVM = new List<ListarProductosViewModel>();
            var productos = repositorioProductos.ListarProductosRegistrados();
                productosVM = productos.Select(p =>{
                var nombreProveedor = repositorioProveedores.ObtenerDetallesDeProveedorPorId(p.IdProveedor).Proveedor;
                return new ListarProductosViewModel(p, nombreProveedor);
            }).ToList();
            return View(productosVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargó la lista de productos correctamente";
            return RedirectToAction("Index");
        }
    }
    [HttpGet]
    public IActionResult AltaProducto(){
        try{
            var proveedores = repositorioProveedores.ListarProveedores();
            var proveedoresVM = proveedores.Select(p => new ListarProveedoresViewModel(p)).ToList();
            var model = new AltaProductoViewModel(proveedoresVM);
            return View(model);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargó el formulario de creación de producto correctamente";
            return RedirectToAction("Index");
        }
    }
    [HttpPost]
    public IActionResult AltaProducto(AltaProductoViewModel productoVM){
        try{
            if(ModelState.IsValid){
                Productos producto = new Productos(productoVM);
                repositorioProductos.CrearNuevoProducto(producto);
                return RedirectToAction("Index");
            }
            return View(productoVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo crear el producto: " + e.Message;
            if (e.InnerException != null)
                ViewBag.ErrorMessage += " | Detalle: " + e.InnerException.Message;
            var proveedores = repositorioProveedores.ListarProveedores();
            var proveedoresVM = proveedores.Select(p => new ListarProveedoresViewModel(p)).ToList();
            productoVM.Proveedores = proveedoresVM;
            return View(productoVM);
        }
    }
    [HttpGet]
    public IActionResult ModificarProducto(int id){
        try{
            Productos producto = repositorioProductos.ObtenerDetallesDeProductoPorId(id);
            var proveedores = repositorioProveedores.ListarProveedores();
            var provedoresVM = proveedores.Select(p => new ListarProveedoresViewModel(p)).ToList();
            ModificarProductoViewModel productoVM = new ModificarProductoViewModel(producto, provedoresVM);
            return View(productoVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el producto";
            return RedirectToAction("Index");
        }
    }
    [HttpPost]
    public IActionResult ModificarProducto(ModificarProductoViewModel productoVM){
        try{
            if(ModelState.IsValid){
                Productos producto = new Productos(productoVM);
                repositorioProductos.ModificarProducto(producto);
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo modificar el producto: " + e.Message;
            if (e.InnerException != null)
                ViewBag.ErrorMessage += " | Detalle: " + e.InnerException.Message;
            var proveedores = repositorioProveedores.ListarProveedores();
            var provedoresVM = proveedores.Select(p => new ListarProveedoresViewModel(p)).ToList();
            productoVM.Proveedores = provedoresVM;
            return View(productoVM);
        }
    }
    [HttpGet]
    public IActionResult EliminarProducto(int id){
        try{
            var producto = repositorioProductos.ObtenerDetallesDeProductoPorId(id);
            var nombreProveedor = repositorioProveedores.ObtenerDetallesDeProveedorPorId(producto.IdProveedor).Proveedor;
            var productoVM = new ListarProductosViewModel(producto, nombreProveedor);
            return View(productoVM);
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el producto";
            return RedirectToAction("Index");
        }
    }
    [HttpPost]
    public IActionResult EliminarProducto(ListarProductosViewModel productoVM){
        try{
            repositorioProductos.EliminarProductoPorId(productoVM.IdProducto);
            return RedirectToAction("Index");
        }catch(Exception e){
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo eliminar el producto";
            return RedirectToAction("Index");
        }
    }
}