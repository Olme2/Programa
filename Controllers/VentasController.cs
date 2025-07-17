using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VentasVM;
public class VentasController : Controller
{
    private readonly IVentaRepository _ventaRepo;
    private readonly IMetodosPagoRepository _metodosPagoRepo;
    private readonly IDetalleVentaRepository _detalleVentaRepo;

    public VentasController(IVentaRepository ventaRepo, IMetodosPagoRepository metodosPagoRepo, IDetalleVentaRepository detalleVentaRepo)
    {
        _ventaRepo = ventaRepo;
        _metodosPagoRepo = metodosPagoRepo;
        _detalleVentaRepo = detalleVentaRepo;
    }

    public IActionResult Index(FiltrarVentasVM filters)
    {
        DateOnly fechaInicio = filters?.FechaInicio ?? DateOnly.FromDateTime(DateTime.Today);
        DateOnly fechaFin = filters?.FechaFin ?? DateOnly.FromDateTime(DateTime.Today);
        var ventas = _ventaRepo.ListarVentasEntreDosFechas(fechaInicio, fechaFin);
        var viewModel = ventas.Select(v => new ListarVentasVM
        {
            IdVenta = v.IdVenta,
            Total = v.Total,
            Fecha = v.Fecha,
            Hora = v.Hora,
            MetodoPagoNombre = "",
            Costo = v.Costo,
            Ganancia = v.Ganancia,
            PorcentajeGanancia = v.PorcentajeGanancia
        }).ToList();
        return View(viewModel);
    }

    // GET: /Ventas/AltaVenta
    public IActionResult AltaVenta()
    {
        var metodos = _metodosPagoRepo.ListarMetodosDePagoRegistrados();
        var vm = new AltaVentaVM
        {
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            Hora = TimeOnly.FromDateTime(DateTime.Now),
            MetodosPago = metodos
        };
        return View(vm);
    }

    // POST: /Ventas/AltaVenta
    [HttpPost]
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
    }

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
    public IActionResult EliminarVenta(long id)
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
    }

    // POST: /Ventas/EliminarVentaConfirmado/{id}
    [HttpPost, ActionName("EliminarVentaConfirmado")]
    public IActionResult EliminarVentaConfirmado(long id)
    {
        _ventaRepo.EliminarVentaPorId(id);
        return RedirectToAction("Index");
    }

    // GET: /Ventas/VerDetalles/{id}
    public IActionResult VerDetalles(long id)
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
    }
} 