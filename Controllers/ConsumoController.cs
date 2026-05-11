using ConsumoVM;
using Microsoft.AspNetCore.Mvc;

public class ConsumoController : Controller
{
    private readonly IConsumoRepository _repo;
    private readonly IProductosRepository _productosRepo;
    private readonly ILogger<ConsumoController> _logger;

    public ConsumoController(IConsumoRepository repo, IProductosRepository productosRepo, ILogger<ConsumoController> logger)
    {
        _repo = repo;
        _productosRepo = productosRepo;
        _logger = logger;
    }

    // GET: Consumo/Index
    [HttpGet]
    public IActionResult Index(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var inicio = fechaInicio ?? DateTime.Today.AddDays(-30);
        var fin    = fechaFin    ?? DateTime.Today;
        var vm = new IndexConsumoVM
        {
            FechaInicio = inicio,
            FechaFin    = fin,
            Consumos    = _repo.ObtenerListado(inicio, fin).ToList(),
        };
        return View(vm);
    }

    // GET: Consumo/Alta
    [HttpGet]
    public IActionResult Alta()
    {
        var vm = new AltaConsumoVM
        {
            ProductosActivos = _productosRepo.ObtenerListadoProductos()
                .Where(p => p.Activo)
                .OrderBy(p => p.Producto)
                .ToList(),
        };
        return View(vm);
    }

    // POST: Consumo/Alta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaConsumoVM vm)
    {
        try
        {
            if (!TieneDetallesValidos(vm.Detalles))
            {
                ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto.");
                return View(vm);
            }
            if (TieneProductosDuplicados(vm.Detalles))
            {
                ModelState.AddModelError(string.Empty, "No se puede agregar el mismo producto mas de una vez.");
                RepoblarDetalles(vm.Detalles);
                return View(vm);
            }
            _repo.Crear(vm);
            TempData["SuccessMessage"] = "Consumo registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al registrar consumo");
            TempData["ErrorMessage"] = "Error al registrar el consumo: " + e.Message;
            RepoblarDetalles(vm.Detalles);
            return View(vm);
        }
    }

    [HttpGet]
    public IActionResult Modificar(int id)
    {
        var consumo = _repo.ObtenerPorId(id);
        if (consumo == null)
        {
            TempData["ErrorMessage"] = "El consumo no fue encontrado.";
            return RedirectToAction(nameof(Index));
        }

        return View(new ModificarConsumoVM(consumo));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(ModificarConsumoVM vm)
    {
        try
        {
            if (!TieneDetallesValidos(vm.Detalles))
            {
                ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto.");
                return View(vm);
            }
            if (TieneProductosDuplicados(vm.Detalles))
            {
                ModelState.AddModelError(string.Empty, "No se puede agregar el mismo producto mas de una vez.");
                RepoblarDetalles(vm.Detalles);
                return View(vm);
            }

            _repo.Actualizar(vm);
            TempData["SuccessMessage"] = "Consumo modificado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar consumo {Id}", vm.IdConsumo);
            TempData["ErrorMessage"] = "Error al modificar el consumo: " + e.Message;
            RepoblarDetalles(vm.Detalles);
            return View(vm);
        }
    }

    // POST: Consumo/Eliminar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        try
        {
            _repo.Eliminar(id);
            TempData["SuccessMessage"] = "Consumo eliminado. El stock fue devuelto.";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar consumo {Id}", id);
            TempData["ErrorMessage"] = "Error al eliminar: " + e.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // GET AJAX: Consumo/ObtenerCostoProducto?idProducto=5
    [HttpGet]
    public IActionResult ObtenerCostoProducto(int idProducto)
    {
        var p = _productosRepo.ObtenerPorId(idProducto);
        if (p == null) return NotFound();
        return Json(new { costo = p.Costo, stock = p.Stock });
    }

    [HttpGet]
    public IActionResult ObtenerVistaDetalleConsumo()
    {
        return PartialView("Partials/_DetalleConsumoItem", new DetalleConsumoVM());
    }

    [HttpGet]
    public IActionResult BuscarProductosParaConsumo(string term, [FromQuery] int[] excluir)
    {
        var query = _productosRepo.ObtenerListadoProductos().Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(term))
            query = query.Where(p => p.Producto.Contains(term, StringComparison.CurrentCultureIgnoreCase));

        if (excluir != null && excluir.Any())
            query = query.Where(p => !excluir.Contains(p.IdProducto));

        var productos = query
            .OrderByDescending(p => p.VentaSemanal)
            .ThenBy(p => p.Producto)
            .Select(p => new
            {
                id = p.IdProducto,
                text = $"{p.Producto} ($ {p.Costo.ToString("N2", CG.CulturaES)}) - S: {(p.Stock == 0 ? "Sin stock" : p.Stock.ToString("N3", CG.CulturaES))}",
                costo = p.Costo,
                disabled = p.Stock <= 0
            })
            .Take(10)
            .ToList();

        return Json(productos);
    }

    private static bool TieneDetallesValidos(IEnumerable<DetalleConsumoVM>? detalles)
    {
        return detalles != null && detalles.Any(d => d.IdProducto > 0 && d.Cantidad > 0);
    }

    private static bool TieneProductosDuplicados(IEnumerable<DetalleConsumoVM> detalles)
    {
        return detalles
            .Where(d => d.IdProducto > 0)
            .GroupBy(d => d.IdProducto)
            .Any(g => g.Count() > 1);
    }

    private void RepoblarDetalles(IEnumerable<DetalleConsumoVM> detalles)
    {
        foreach (var detalle in detalles.Where(d => d.IdProducto > 0))
        {
            var producto = _productosRepo.ObtenerPorId(detalle.IdProducto);
            if (producto == null) continue;

            detalle.NombreProducto = $"{producto.Producto} ($ {producto.Costo.ToString("N2", CG.CulturaES)}) - S: {producto.Stock.ToString("N3", CG.CulturaES)}";
            detalle.CostoUnitario = producto.Costo;
        }
    }
}
