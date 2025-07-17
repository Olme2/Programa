/*using Microsoft.AspNetCore.Mvc;
public class DetallesVentasController : Controller
{
    private readonly IDetalleVentaRepository _detalleVentaRepo;
    private readonly IProductosRepository _productosRepo;

    public DetallesVentasController(IDetalleVentaRepository detalleVentaRepo, IProductosRepository productosRepo)
    {
        _detalleVentaRepo = detalleVentaRepo;
        _productosRepo = productosRepo;
    }

    public IActionResult AgregarDetalle(long idVenta)
    {
        var productos = _productosRepo.ListarProductosRegistrados();
        var vm = new AltaDetallesVentaVM
        {
            IdVenta = idVenta,
            Productos = productos.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = p.IdProducto.ToString(), Text = p.Producto }).ToList()
        };
        return View(vm);
    }

    // POST: /DetallesVentas/AgregarDetalle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AgregarDetalle(AltaDetallesVentaVM model)
    {
        if (!ModelState.IsValid)
        {
            var productos = _productosRepo.ListarProductosRegistrados();
            model.Productos = productos.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = p.IdProducto.ToString(), Text = p.Producto }).ToList();
            return View(model);
        }
        var detalle = new DetallesVentas
        {
            IdVenta = model.IdVenta,
            IdProducto = model.IdProducto,
            Cantidad = model.Cantidad,
            Promocion = model.Promocion,
            PrecioPromo = model.PrecioPromo,
            CostoPromo = model.CostoPromo
        };
        _detalleVentaRepo.Add(detalle);
        return RedirectToAction("VerDetalles", "Ventas", new { id = model.IdVenta });
    }

    // GET: /DetallesVentas/ModificarDetalle/{idVenta}/{idProducto}
    public IActionResult ModificarDetalle(long idVenta, int idProducto)
    {
        var detalle = _detalleVentaRepo.GetById(idVenta, idProducto);
        if (detalle == null) return NotFound();
        var productos = _productosRepo.ListarProductosRegistrados();
        var vm = new AltaDetallesVentaVM
        {
            IdVenta = detalle.IdVenta,
            IdProducto = detalle.IdProducto,
            Cantidad = detalle.Cantidad,
            Promocion = detalle.Promocion,
            PrecioPromo = detalle.PrecioPromo,
            CostoPromo = detalle.CostoPromo,
            Productos = productos.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = p.IdProducto.ToString(), Text = p.Producto }).ToList()
        };
        return View(vm);
    }

    // POST: /DetallesVentas/ModificarDetalle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ModificarDetalle(AltaDetallesVentaVM model)
    {
        if (!ModelState.IsValid)
        {
            var productos = _productosRepo.ListarProductosRegistrados();
            model.Productos = productos.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = p.IdProducto.ToString(), Text = p.Producto }).ToList();
            return View(model);
        }
        var detalle = _detalleVentaRepo.GetById(model.IdVenta, model.IdProducto);
        if (detalle == null) return NotFound();
        detalle.Cantidad = model.Cantidad;
        detalle.Promocion = model.Promocion;
        detalle.PrecioPromo = model.PrecioPromo;
        detalle.CostoPromo = model.CostoPromo;
        _detalleVentaRepo.Update(detalle);
        return RedirectToAction("VerDetalles", "Ventas", new { id = model.IdVenta });
    }

    // GET: /DetallesVentas/EliminarDetalle/{idVenta}/{idProducto}
    public IActionResult EliminarDetalle(long idVenta, int idProducto)
    {
        var detalle = _detalleVentaRepo.GetById(idVenta, idProducto);
        if (detalle == null) return NotFound();
        var vm = new DetallesVentaVM
        {
            IdVenta = detalle.IdVenta,
            IdProducto = detalle.IdProducto,
            ProductoNombre = "", // Mapear nombre real si es necesario
            Cantidad = detalle.Cantidad,
            Promocion = detalle.Promocion,
            PrecioPromo = detalle.PrecioPromo,
            CostoPromo = detalle.CostoPromo
        };
        ViewBag.IdVenta = detalle.IdVenta;
        ViewBag.IdProducto = detalle.IdProducto;
        return View(vm);
    }

    // POST: /DetallesVentas/EliminarDetalle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarDetalle(DetallesVentaVM model)
    {
        _detalleVentaRepo.Delete(model.IdVenta, model.IdProducto);
        return RedirectToAction("VerDetalles", "Ventas", new { id = model.IdVenta });
    }
} */