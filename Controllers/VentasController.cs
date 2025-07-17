using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VentasVM;
public class VentasController : Controller
{
    private readonly ILogger _logger;
    private readonly IVentaRepository _ventaRepo;
    private readonly IMetodosPagoRepository _metodosPagoRepo;
    private readonly IProductosRepository _productosRepo;

    public VentasController(ILogger logger, IVentaRepository ventaRepo, IMetodosPagoRepository metodosPagoRepo, IProductosRepository productosRepo)
    {
        _logger = logger;
        _ventaRepo = ventaRepo;
        _metodosPagoRepo = metodosPagoRepo;
        _productosRepo = productosRepo;
    }

    [HttpGet]
    public IActionResult Index(FiltrarVentasVM filtros)
    {
        try
        {
            DateOnly fechaInicio = filtros?.FechaInicio ?? DateOnly.FromDateTime(DateTime.Today);
            DateOnly fechaFin = filtros?.FechaFin ?? DateOnly.FromDateTime(DateTime.Today);
            var ventas = _ventaRepo.ListarVentasEntreDosFechas(fechaInicio, fechaFin);
            var viewModel = ventas.Select(v =>
            {
                var productosVendidos = v.DetallesVenta.Select(d => _productosRepo.ObtenerDetallesDeProductoPorId(d.IdProducto).Producto).ToList(); //Obtiene lista de productos string
                var metodoDePago = _metodosPagoRepo.ObtenerDetallesDeMetodoDePagoPorId(v.IdMetodo).Metodo; //Obtiene metodo de pago string
                return new ListarVentasVM(v, metodoDePago, productosVendidos);
            }).ToList();
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se cargo la lista de ventas correctamente";
            return RedirectToAction("AltaVenta", "Ventas");
        }
    }

    [HttpGet]
    public IActionResult AltaVenta()
    {
        var metodos = _metodosPagoRepo.ListarMetodosDePagoRegistrados();
        var vm = new AltaVentaVM(metodos);
        return View(vm);
    }

    // POST: /Ventas/AltaVenta
    /*[HttpPost]
    public IActionResult AltaVenta(AltaVentaVM model)
    {
        if (!ModelState.IsValid)
        {
            var metodos = _metodosPagoRepo.ListarMetodosDePagoRegistrados();
            model.MetodosPago = metodos;
            return View(model);
        }
        var venta = new Ventas
        {
            IdMetodo = model.IdMetodoPago,
            Detalle = model.Detalle,
            Fecha = model.Fecha,
            Hora = model.Hora,
            Total = 0, // El trigger de la BD lo calculará
            Costo = 0,
            Ganancia = 0,
            PorcentajeGanancia = 0
        };
        _ventaRepo.CrearNuevaVenta(venta);
        return RedirectToAction("Index");
    }*/

    // GET: /Ventas/ModificarVenta/{id}
    public IActionResult ModificarVenta(long id)
    {
        var venta = _ventaRepo.ObtenerDetallesDeVentaPorId(id);
        if (venta == null) return NotFound();
        var metodos = _metodosPagoRepo.ListarMetodosDePagoRegistrados();
        var vm = new ModificarVentaVM
        {
            IdVenta = venta.IdVenta,
            IdMetodoPago = venta.IdMetodo,
            Detalle = venta.Detalle,
            Fecha = venta.Fecha,
            Hora = venta.Hora,
            MetodosPago = metodos.Select(m => new SelectListItem { Value = m.IdMetodo.ToString(), Text = m.Metodo }).ToList()
        };
        return View(vm);
    }

    // POST: /Ventas/ModificarVenta
    [HttpPost]
    public IActionResult ModificarVenta(ModificarVentaVM model)
    {
        if (!ModelState.IsValid)
        {
            var metodos = _metodosPagoRepo.ListarMetodosDePagoRegistrados();
            model.MetodosPago = metodos.Select(m => new SelectListItem { Value = m.IdMetodo.ToString(), Text = m.Metodo }).ToList();
            return View(model);
        }
        var venta = _ventaRepo.ObtenerDetallesDeVentaPorId(model.IdVenta);
        if (venta == null) return NotFound();
        venta.IdMetodo = model.IdMetodoPago;
        venta.Detalle = model.Detalle;
        venta.Fecha = model.Fecha;
        venta.Hora = model.Hora;
        _ventaRepo.ModificarVenta(venta);
        return RedirectToAction("Index");
    }

    // GET: /Ventas/EliminarVenta/{id}
    /*public IActionResult EliminarVenta(long id)
    {
        var venta = _ventaRepo.ObtenerDetallesDeVentaPorId(id);
        if (venta == null) return NotFound();
        var detalles = _detalleVentaRepo.GetByVentaId(id);
        var vm = new VerVentaVM
        {
            IdVenta = venta.IdVenta,
            Total = venta.Total,
            Fecha = venta.Fecha,
            Hora = venta.Hora,
            MetodoPagoNombre = "", // Se puede mapear si se requiere
            Costo = venta.Costo,
            Ganancia = venta.Ganancia,
            PorcentajeGanancia = venta.PorcentajeGanancia,
            Detalle = venta.Detalle,
            Detalles = detalles.Select(d => new DetallesVentasVM.ListarDetallesVentaVM
            {
                IdVenta = d.IdVenta,
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                Promocion = d.Promocion,
                PrecioPromo = d.PrecioPromo,
                CostoPromo = d.CostoPromo
            }).ToList()
        };
        return View(vm);
    }*/

    // POST: /Ventas/EliminarVentaConfirmado/{id}
    [HttpPost, ActionName("EliminarVentaConfirmado")]
    public IActionResult EliminarVentaConfirmado(long id)
    {
        _ventaRepo.EliminarVentaPorId(id);
        return RedirectToAction("Index");
    }

    // GET: /Ventas/VerDetalles/{id}
    /*public IActionResult VerDetalles(long id)
    {
        var venta = _ventaRepo.ObtenerDetallesDeVentaPorId(id);
        if (venta == null) return NotFound();
        var detalles = _detalleVentaRepo.GetByVentaId(id);
        var vm = new VerVentaVM
        {
            IdVenta = venta.IdVenta,
            Total = venta.Total,
            Fecha = venta.Fecha,
            Hora = venta.Hora,
            MetodoPagoNombre = "", // Se puede mapear si se requiere
            Costo = venta.Costo,
            Ganancia = venta.Ganancia,
            PorcentajeGanancia = venta.PorcentajeGanancia,
            Detalle = venta.Detalle,
            Detalles = detalles.Select(d => new DetallesVentasVM.ListarDetallesVentaVM
            {
                IdVenta = d.IdVenta,
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                Promocion = d.Promocion,
                PrecioPromo = d.PrecioPromo,
                CostoPromo = d.CostoPromo
            }).ToList()
        };
        return View(vm);
    }*/
} 

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