using Microsoft.AspNetCore.Mvc;
using PromocionesVM;
public class PromocionesController : Controller
{
    private readonly ILogger<ProveedoresController> _logger;
    private IPromocionesRepository _promocionesRepo;
    private IProveedoresRepository _proveedoresRepo;

    public ProveedoresController(ILogger<ProveedoresController> logger, IPromocionesRepository promocionesRepo, IProveedoresRepository proveedoresRepo)
    {
        _logger = logger;
        _promocionesRepo = promocionesRepo;
        _proveedoresRepo = proveedoresRepo;
    }
}