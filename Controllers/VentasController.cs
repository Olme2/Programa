using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using entornoPolleria;

public class VentasController : Controller
{
    private readonly ILogger<VentasController> _logger;
    private readonly IVentasRepository _ventasRepo;
    private readonly IProductosRepository _productosRepo;
    private readonly IProveedoresRepository _proveedoresRepo;
    private readonly AppDbContext _context;

    public VentasController(ILogger<VentasController> logger, IVentasRepository ventasRepo, IProductosRepository productosRepo, IProveedoresRepository proveedoresRepo, AppDbContext context)
    {
        _logger = logger;
        _ventasRepo = ventasRepo;
        _productosRepo = productosRepo;
        _proveedoresRepo = proveedoresRepo;
        _context = context;
    }

    public IActionResult Index(DateOnly? fecha)
    {
        try
        {
            DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);
            DateOnly fechaFiltro = fecha ?? hoy;
            var ventas = _ventasRepo.ListarVentas()
                .Where(v => v.Fecha == fechaFiltro)
                .ToList();
            var ventasVM = ventas.Select(v => new VentaConDetallesViewModel
            {
                IdVenta = v.IdVenta,
                MetodoDePago = v.MetodoDePago?.Metodo ?? "-",
                Total = v.Total,
                Costo = v.Costo,
                Ganancia = v.Ganancia,
                PorcentajeGanancia = v.PorcentajeGanancia,
                Fecha = v.Fecha,
                Hora = v.Hora,
                Detalle = v.Detalle
            }).ToList();
            ViewBag.FechaSeleccionada = fechaFiltro;
            return View(ventasVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el listado de ventas.";
            return View(new List<VentaConDetallesViewModel>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        try
        {
            var productos = _productosRepo.ListarProductosRegistrados();
            var metodos = _context.MetodosDePago.ToList();
            var model = new AltaVentaViewModel
            {
                MetodosDePago = metodos
            };
            // Cargar productos disponibles para el formulario
            ViewBag.Productos = productos;
            return View(model);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el formulario de venta.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult Create(AltaVentaViewModel ventaVM)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ventaVM.MetodosDePago = _context.MetodosDePago.ToList();
                ViewBag.Productos = _productosRepo.ListarProductosRegistrados();
                return View(ventaVM);
            }
            // Mapear detalles
            var detalles = ventaVM.Detalles.Select(d => new DetallesVentas
            {
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                Promocion = d.Promocion,
                PrecioPromo = d.PrecioPromo,
                CostoPromo = d.CostoPromo
            }).ToList();
            var venta = new Ventas
            {
                IdMetodo = ventaVM.IdMetodo ?? 0,
                Detalle = ventaVM.Detalle
            };
            _ventasRepo.CrearVenta(venta, detalles);
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "Error al crear la venta: " + e.Message;
            ventaVM.MetodosDePago = _context.MetodosDePago.ToList();
            ViewBag.Productos = _productosRepo.ListarProductosRegistrados();
            return View(ventaVM);
        }
    }

    [HttpGet]
    public IActionResult Edit(long id)
    {
        try
        {
            var venta = _ventasRepo.ObtenerVentaPorId(id);
            if (venta == null) return RedirectToAction("Index");
            var metodos = _context.MetodosDePago.ToList();
            var detallesVM = venta.Detalles?.Select(d => new DetalleVentaViewModel
            {
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                Promocion = d.Promocion,
                PrecioPromo = d.PrecioPromo,
                CostoPromo = d.CostoPromo,
                NombreProducto = d.Producto?.Producto,
                StockDisponible = d.Producto?.Stock ?? 0
            }).ToList() ?? new List<DetalleVentaViewModel>();
            var model = new AltaVentaViewModel
            {
                IdMetodo = venta.IdMetodo,
                MetodosDePago = metodos,
                Detalle = venta.Detalle,
                Detalles = detallesVM
            };
            ViewBag.Productos = _productosRepo.ListarProductosRegistrados();
            ViewBag.IdVenta = id;
            return View(model);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar la venta para edición.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public IActionResult Edit(long id, AltaVentaViewModel ventaVM)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                ventaVM.MetodosDePago = _context.MetodosDePago.ToList();
                ViewBag.Productos = _productosRepo.ListarProductosRegistrados();
                ViewBag.IdVenta = id;
                return View(ventaVM);
            }
            var detalles = ventaVM.Detalles.Select(d => new DetallesVentas
            {
                IdVenta = id,
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                Promocion = d.Promocion,
                PrecioPromo = d.PrecioPromo,
                CostoPromo = d.CostoPromo
            }).ToList();
            var venta = new Ventas
            {
                IdVenta = id,
                IdMetodo = ventaVM.IdMetodo ?? 0,
                Detalle = ventaVM.Detalle
            };
            _ventasRepo.ModificarVenta(venta, detalles);
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "Error al modificar la venta: " + e.Message;
            ventaVM.MetodosDePago = _context.MetodosDePago.ToList();
            ViewBag.Productos = _productosRepo.ListarProductosRegistrados();
            ViewBag.IdVenta = id;
            return View(ventaVM);
        }
    }

    [HttpGet]
    public IActionResult Delete(long id)
    {
        try
        {
            var venta = _ventasRepo.ObtenerVentaPorId(id);
            if (venta == null) return RedirectToAction("Index");
            var ventaVM = new VentaConDetallesViewModel
            {
                IdVenta = venta.IdVenta,
                MetodoDePago = venta.MetodoDePago?.Metodo ?? "-",
                Total = venta.Total,
                Costo = venta.Costo,
                Ganancia = venta.Ganancia,
                PorcentajeGanancia = venta.PorcentajeGanancia,
                Fecha = venta.Fecha,
                Hora = venta.Hora,
                Detalle = venta.Detalle,
                Detalles = venta.Detalles?.Select(d => new DetalleVentaViewModel
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    Promocion = d.Promocion,
                    PrecioPromo = d.PrecioPromo,
                    CostoPromo = d.CostoPromo,
                    NombreProducto = d.Producto?.Producto,
                    StockDisponible = d.Producto?.Stock ?? 0
                }).ToList() ?? new List<DetalleVentaViewModel>()
            };
            return View(ventaVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar la venta para eliminar.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(long id)
    {
        try
        {
            _ventasRepo.EliminarVenta(id);
            return RedirectToAction("Index");
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "Error al eliminar la venta: " + e.Message;
            return RedirectToAction("Index");
        }
    }

    public IActionResult Details(long id)
    {
        try
        {
            var venta = _ventasRepo.ObtenerVentaPorId(id);
            if (venta == null) return RedirectToAction("Index");
            var detallesVM = venta.Detalles?.Select(d => new DetalleVentaViewModel
            {
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                Promocion = d.Promocion,
                PrecioPromo = d.PrecioPromo,
                CostoPromo = d.CostoPromo,
                NombreProducto = d.Producto?.Producto,
                StockDisponible = d.Producto?.Stock ?? 0
            }).ToList() ?? new List<DetalleVentaViewModel>();
            var ventaVM = new VentaConDetallesViewModel
            {
                IdVenta = venta.IdVenta,
                MetodoDePago = venta.MetodoDePago?.Metodo ?? "-",
                Total = venta.Total,
                Costo = venta.Costo,
                Ganancia = venta.Ganancia,
                PorcentajeGanancia = venta.PorcentajeGanancia,
                Fecha = venta.Fecha,
                Hora = venta.Hora,
                Detalle = venta.Detalle,
                Detalles = detallesVM
            };
            return View(ventaVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            ViewBag.ErrorMessage = "No se pudo cargar el detalle de la venta.";
            return RedirectToAction("Index");
        }
    }
}
