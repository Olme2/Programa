using ProduccionVM;
using Microsoft.AspNetCore.Mvc;

public class ProduccionController : Controller
{
    private readonly IProduccionRepository _repo;
    private readonly IProductosRepository _productosRepo;
    private readonly ILogger<ProduccionController> _logger;

    public ProduccionController(IProduccionRepository repo, IProductosRepository productosRepo, ILogger<ProduccionController> logger)
    {
        _repo = repo;
        _productosRepo = productosRepo;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index(DateTime? fechaInicio, DateTime? fechaFin)
    {
        var inicio = fechaInicio ?? DateTime.Today.AddDays(-30);
        var fin = fechaFin ?? DateTime.Today;
        var vm = new IndexProduccionVM
        {
            FechaInicio = inicio,
            FechaFin = fin,
            Producciones = _repo.ObtenerListado(inicio, fin).ToList(),
        };
        return View(vm);
    }

    [HttpGet]
    public IActionResult Alta()
    {
        return View(new AltaProduccionVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Alta(AltaProduccionVM vm)
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
            TempData["SuccessMessage"] = "Retiro de produccion registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al registrar produccion");
            TempData["ErrorMessage"] = "Error al registrar: " + e.Message;
            RepoblarDetalles(vm.Detalles);
            return View(vm);
        }
    }

    [HttpGet]
    public IActionResult Modificar(int id)
    {
        var produccion = _repo.ObtenerPorId(id);
        if (produccion == null)
        {
            TempData["ErrorMessage"] = "El retiro de produccion no fue encontrado.";
            return RedirectToAction(nameof(Index));
        }

        return View(new ModificarProduccionVM(produccion));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modificar(ModificarProduccionVM vm)
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
            TempData["SuccessMessage"] = "Retiro de produccion modificado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al modificar produccion {Id}", vm.IdProduccion);
            TempData["ErrorMessage"] = "Error al modificar: " + e.Message;
            RepoblarDetalles(vm.Detalles);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Eliminar(int id)
    {
        try
        {
            _repo.Eliminar(id);
            TempData["SuccessMessage"] = "Retiro eliminado. El stock fue devuelto.";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al eliminar produccion {Id}", id);
            TempData["ErrorMessage"] = "Error al eliminar: " + e.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ObtenerVistaDetalleProduccion()
    {
        return PartialView("Partials/_DetalleProduccionItem", new DetalleProduccionVM());
    }

    [HttpGet]
    public IActionResult BuscarProductosParaProduccion(string term, [FromQuery] int[] excluir)
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

    private static bool TieneDetallesValidos(IEnumerable<DetalleProduccionVM>? detalles)
    {
        return detalles != null && detalles.Any(d => d.IdProducto > 0 && d.Cantidad > 0);
    }

    private static bool TieneProductosDuplicados(IEnumerable<DetalleProduccionVM> detalles)
    {
        return detalles
            .Where(d => d.IdProducto > 0)
            .GroupBy(d => d.IdProducto)
            .Any(g => g.Count() > 1);
    }

    private void RepoblarDetalles(IEnumerable<DetalleProduccionVM> detalles)
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
