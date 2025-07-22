using DetallesVentasVM;
using MetodosVM;
using System.ComponentModel.DataAnnotations;
using ProductosVM;
using VentasPromocionesVM;
using PromocionesVM;
namespace VentasVM;

public class ModificarVentaVM
{
    [Required]
    public long IdVenta { get; set; }
    [Required(ErrorMessage = "Debe seleccionar un método de pago.")]
    [Display(Name = "Método de Pago")]
    public short IdMetodo { get; set; }
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly Fecha { get; set; }
    [Required(ErrorMessage = "La hora es obligatoria.")]
    public TimeOnly Hora { get; set; }
    public string? Detalle { get; set; }
    [MinLength(1, ErrorMessage = "La venta debe tener al menos un ítem.")]
    public List<ModificarDetalleVentaVM> DetallesVenta { get; set; }
    [MinLength(1, ErrorMessage = "La venta debe tener al menos un ítem.")]
    public List<ModificarVentaPromocionVM> VentaPromociones { get; set; }
    public List<ListarProductosVM> Productos { get; set; }
    public List<ListarPromocionesVM> Promociones { get; set; }
    public List<ListarMetodosDePagoVM> Metodos { get; set; }

    public ModificarVentaVM()
    {
        DetallesVenta = new List<ModificarDetalleVentaVM>();
        VentaPromociones = new List<ModificarVentaPromocionVM>();
        Productos = new List<ListarProductosVM>();
        Promociones = new List<ListarPromocionesVM>();
        Metodos = new List<ListarMetodosDePagoVM>();
    }
    public ModificarVentaVM(List<ListarProductosVM> productos, List<ListarPromocionesVM> promociones, List<ListarMetodosDePagoVM> metodos) : this()
    {
        Productos = productos;
        Promociones = promociones;
        Metodos = metodos;
    }
}