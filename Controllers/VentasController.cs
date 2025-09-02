using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VentasVM; // Namespace para los ViewModels de Venta
using VentasPromocionesVM;
using DetallesVentasVM;
using System.Globalization;
using TempDataExtension;

public class VentasController : Controller
{
    private readonly IVentaRepository _ventaRepo;
    private readonly IProductosRepository _productosRepo;
    private readonly IPromocionesRepository _promocionesRepo;
    private readonly IMetodosPagoRepository _metodosPagoRepo;
    private readonly ILogger<VentasController> _logger;

    public VentasController(
        IVentaRepository ventaRepo,
        IProductosRepository productosRepo,
        IPromocionesRepository promocionesRepo,
        IMetodosPagoRepository metodosPagoRepo,
        ILogger<VentasController> logger)
    {
        _ventaRepo = ventaRepo;
        _productosRepo = productosRepo;
        _promocionesRepo = promocionesRepo;
        _metodosPagoRepo = metodosPagoRepo;
        _logger = logger;
    }

    // GET: /Ventas
    [HttpGet]
    public IActionResult Index(IndexVentasVM filtro)
    {
        try
        {
            var horaActual = TimeOnly.FromDateTime(DateTime.Now);
            var viewModel = TempData.Get<IndexVentasVM>("FiltrosVentas");
            if (viewModel == null)
            {
                viewModel = new IndexVentasVM()
                {
                    FechaInicio = filtro.FechaInicio == default ? DateTime.Today : filtro.FechaInicio,
                    FechaFin = filtro.FechaFin == default ? DateTime.Today : filtro.FechaFin,
                    Busqueda = filtro.Busqueda,
                    IdMetodoPago = filtro.IdMetodoPago,
                };
                viewModel.Turno = horaActual switch
                {
                    var h when h >= new TimeOnly(9, 0) && h <= new TimeOnly(14, 0) => IndexVentasVM.Turnos.Mañana,
                    var h when h >= new TimeOnly(17, 30) && h <= new TimeOnly(21, 30) => IndexVentasVM.Turnos.Tarde,
                    _ => IndexVentasVM.Turnos.Todos
                };
            }
            viewModel.Ventas = _ventaRepo.ObtenerListadoVentas(viewModel).ToList();
            viewModel.MetodosPago = _metodosPagoRepo.ObtenerListadoMetodosPago().ToList();
            TempData.Set("FiltrosVentas", viewModel);
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener el listado de ventas.");
            TempData["ErrorMessage"] = "No se pudo cargar el listado de ventas.";
            return View(new IndexVentasVM());
        }
    }

    [HttpGet]
    public IActionResult _BuscarVentas(IndexVentasVM filtro)
    {
        try
        {
            TempData.Set("FiltrosVentas", filtro);
            var ventas = _ventaRepo.ObtenerListadoVentas(filtro);
            ViewData["BusquedaActual"] = filtro.Busqueda;
            return PartialView("_VentasTabla", ventas.ToList());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error en la búsqueda dinámica de ventas.");
            return StatusCode(500);
        }
    }

    [HttpGet]
    public IActionResult ResetearFiltros()
    {
        // 1. La única responsabilidad de esta acción es limpiar los filtros guardados en TempData.
        TempData.Remove("FiltrosVentas");

        // 2. Redirigimos de vuelta al Index. La lógica del Index se encargará del resto.
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Alta()
    {
        try
        {
            var viewModel = new AltaVentaVM();
            viewModel.MetodosPago = ObtenerListaMetodosDePago();
            ViewBag.FiltrosAnteriores = Request.Query;
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al preparar el formulario de alta de venta.");
            TempData["ErrorMessage"] = "Ocurrió un error al preparar el formulario.";
            return RedirectToAction("Index", "Home");
        }
    }

    // GET: /Ventas/Alta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaVentaVM viewModel)
    {
        // Tu lógica para normalizar decimales es correcta
        viewModel.Recargo = decimal.Parse(viewModel.Recargo.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        if (viewModel.DetallesVenta != null)
        {
            RepoblarViewModelParaAltaVenta(viewModel);
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var nuevaVenta = Ventas.CrearDesdeViewModel(viewModel);
            _ventaRepo.Crear(nuevaVenta);

            TempData["SuccessMessage"] = "Venta registrada con éxito.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación de negocio al crear la venta.");
            TempData["ErrorMessage"] = ex.Message;
            RepoblarViewModelParaAltaVenta(viewModel);
            return View(viewModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inesperado al crear la venta.");
            TempData["ErrorMessage"] = "Ocurrió un error inesperado al registrar la venta.";
            RepoblarViewModelParaAltaVenta(viewModel);
            return View(viewModel);
        }
    }

    // --- NUEVO MÉTODO DE AYUDA PRIVADO ---
    private void RepoblarViewModelParaAltaVenta(AltaVentaVM viewModel)
    {
        viewModel.MetodosPago = ObtenerListaMetodosDePago();
        if (viewModel.DetallesVenta != null)
        {
            foreach (var detalle in viewModel.DetallesVenta)
            {
                if (detalle.IdProducto > 0)
                {
                    var producto = _productosRepo.ObtenerPorId(detalle.IdProducto);
                    if (producto != null)
                    {
                        detalle.IdProducto = producto.IdProducto;
                        detalle.NombreProducto = $"{producto.Producto} (${producto.Precio.ToString("N2", CG.CulturaES)}) - S: {producto.Stock.ToString("N2", CG.CulturaES)}";
                        detalle.PrecioUnitario = producto.Precio;
                        detalle.CostoUnitario = producto.Costo;
                    }
                }
            }
        }

        // Repoblamos los detalles de promociones
        if (viewModel.VentaPromociones != null)
    {
        foreach (var detallePromo in viewModel.VentaPromociones)
        {
            if (detallePromo.IdPromocion > 0)
            {
                var promocion = _promocionesRepo.ObtenerListadoPromociones().FirstOrDefault(p => p.IdPromocion == detallePromo.IdPromocion);
                if (promocion != null)
                {
                    detallePromo.IdPromocion = promocion.IdPromocion;
                    detallePromo.NombrePromocion = $"{promocion.Promocion} (${promocion.Precio.ToString("N2", CG.CulturaES)}) - S: {promocion.Stock.ToString("N2", CG.CulturaES)}";
                    detallePromo.PrecioPromo = promocion.Precio;
                    detallePromo.CostoPromo = promocion.Costo;
                }
            }
        }
    }
    }

    private List<SelectListItem> ObtenerListaMetodosDePago()
    {
        return _metodosPagoRepo.ObtenerListadoMetodosPago()
           .Select(m => new SelectListItem { Value = m.IdMetodo.ToString(), Text = m.Metodo })
           .ToList();
    }

    // --- ACCIONES AJAX PARA LA VISTA ---

    [HttpGet]
    public IActionResult ObtenerVistaDetalleVenta()
    {
        return PartialView("Partials/_DetalleVentaItem", new DetalleVentaVM());
    }

    [HttpGet]
    public IActionResult ObtenerVistaVentaPromocion()
    {
        var vm = new VentaPromocionVM();
        return PartialView("Partials/_VentaPromocionItem", vm);
    }


    [HttpGet]
    public IActionResult BuscarProductosParaVenta(string term, [FromQuery] int[] excluir)
    {
        var query = _productosRepo.ObtenerListadoProductos().Where(p => p.Activo);

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(p => p.Producto.Contains(term, StringComparison.CurrentCultureIgnoreCase));
        }
        if (excluir != null)
        {
            query = query.Where(p => !excluir.Contains(p.IdProducto));
        }

        // --- CORRECCIÓN CLAVE ---
        // Nos aseguramos de devolver 'id', 'text', 'precio' y 'costo'.
        var productos = query.OrderByDescending(p => p.VentaSemanal).ThenBy(p => p.Producto)
            .Select(p => new
            {
                id = p.IdProducto,
                text = $"{p.Producto} (${p.Precio.ToString("N2", CG.CulturaES)}) - S: {(p.Stock == 0 ? "Sin Stock" : p.Stock.ToString("N2", CG.CulturaES))}",
                precio = p.Precio,
                costo = p.Costo,
                disabled = p.Stock <= 0
            })
            .Take(10).ToList();

        return Json(productos);
    }


    [HttpGet]
    public IActionResult BuscarPromocionesParaVenta(string term, [FromQuery] int[] excluir)
    {
        var query = _promocionesRepo.ObtenerListadoPromociones().Where(p => p.Activa); // Asumo que tienes un método así

        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(p => p.Promocion.Contains(term, StringComparison.CurrentCultureIgnoreCase));
        }
        if (excluir != null && excluir.Any())
        {
            query = query.Where(p => !excluir.Contains(p.IdPromocion));
        }

        var promociones = query.OrderByDescending(p => p.VentaSemanal).ThenBy(p => p.Promocion)
            .Select(p => new
            {
                id = p.IdPromocion,
                text = $"{p.Promocion} (${p.Precio.ToString("N2", CG.CulturaES)}) - S: {(p.Stock == 0 ? "Sin Stock" : p.Stock.ToString("N2", CG.CulturaES))}",
                precio = p.Precio,
                costo = p.Costo,
                disabled = p.Stock <= 0
            })
            .Take(10).ToList();

        return Json(promociones);
    }

    [HttpGet]
    public IActionResult Modificar(long id)
    {
        var venta = _ventaRepo.ObtenerPorId(id);
        if (venta == null)
        {
            TempData["ErrorMessage"] = "No existe venta con ese id.";
            return RedirectToAction(nameof(Index));
        }
        var metodos = ObtenerListaMetodosDePago();
        var viewModel = new ModificarVentaVM(venta, metodos);
        ViewBag.FiltrosAnteriores = Request.Query;
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(ModificarVentaVM viewModel)
    {
        // Rehidratamos por si la validación falla
        RepoblarViewModelParaModificarVenta(viewModel);
        if (!ModelState.IsValid)
        {
            RepoblarViewModelParaModificarVenta(viewModel);
            return View(viewModel);
        }

        try
        {
            var venta = _ventaRepo.ObtenerPorId(viewModel.IdVenta);
            if (venta == null)
            {
                TempData["ErrorMessage"] = "No existe venta con ese id.";
                return RedirectToAction(nameof(Index));
            }
            venta.ActualizarDesdeViewModel(viewModel);
            _ventaRepo.Actualizar(venta);
            TempData["SuccessMessage"] = "Venta modificada con éxito.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar la venta.");
            TempData["ErrorMessage"] = "Ocurrió un error al modificar la venta.";
            return View(viewModel);
        }
    }



    // Método de ayuda para rehidratar el VM de Modificar
    private void RepoblarViewModelParaModificarVenta(ModificarVentaVM viewModel)
    {
        viewModel.Metodos = ObtenerListaMetodosDePago();
        if (viewModel.DetallesVenta != null)
        {
            foreach (var detalle in viewModel.DetallesVenta)
            {
                if (detalle.IdProducto > 0)
                {
                    var producto = _productosRepo.ObtenerPorId(detalle.IdProducto);
                    if (producto != null)
                    {
                        detalle.IdProducto = producto.IdProducto;
                        detalle.NombreProducto = $"{producto.Producto} (${producto.Precio.ToString("N2", CG.CulturaES)}) - S: {producto.Stock.ToString("N2", CG.CulturaES)}";
                        detalle.PrecioUnitario = producto.Precio;
                        detalle.CostoUnitario = producto.Costo;
                    }
                }
            }
        }

        // Repoblamos los detalles de promociones
        if (viewModel.VentaPromociones != null)
        {
            foreach (var detallePromo in viewModel.VentaPromociones)
            {
                if (detallePromo.IdPromocion > 0)
                {
                    var promocion = _promocionesRepo.ObtenerListadoPromociones().FirstOrDefault(p => p.IdPromocion == detallePromo.IdPromocion);
                    if (promocion != null)
                    {
                        detallePromo.IdPromocion = promocion.IdPromocion;
                        detallePromo.NombrePromocion = $"{promocion.Promocion} (${promocion.Precio.ToString("N2", CG.CulturaES)}) - S: {promocion.Stock.ToString("N2", CG.CulturaES)}";
                        detallePromo.PrecioPromo = promocion.Precio;
                        detallePromo.CostoPromo = promocion.Costo;
                    }
                }
            }
        }
    }


    [HttpGet]
    public IActionResult Eliminar(long id)
    {
        try
        {
            // Usamos el mismo método del Index, filtrando por el ID específico.
            // Esto nos da el ViewModel que la vista necesita sin escribir lógica de mapeo nueva.
            var venta = _ventaRepo.ObtenerPorId(id);
            if (venta == null)
            {
                TempData["ErrorMessage"] = "La venta que intenta eliminar no fue encontrada.";
                return RedirectToAction(nameof(Index));
            }
            var ventaVM = new ListarVentasVM(venta);
            return View(ventaVM);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener la venta con ID {Id} para eliminar.", id);
            TempData["ErrorMessage"] = "Ocurrió un error al cargar los datos de la venta.";
            return RedirectToAction(nameof(Index));
        }
    }



    // POST: Ventas/Eliminar/5
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public IActionResult EliminarConfirmado(long IdVenta)
    {
        try
        {
            _ventaRepo.Eliminar(IdVenta);
            TempData["SuccessMessage"] = "Venta eliminada con éxito.";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar la venta con ID {Id}", IdVenta);
            TempData["ErrorMessage"] = "Ocurrió un error al intentar eliminar la venta.";
        }

        return RedirectToAction(nameof(Index));
    }

    
}
