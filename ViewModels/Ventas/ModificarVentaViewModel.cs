using DetallesVentasVM;
using MetodosVM;
using System.ComponentModel.DataAnnotations;
using ProductosVM;
using VentasPromocionesVM;
using PromocionesVM;
namespace VentasVM;

public class ModificarVentaVM : IValidatableObject
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
    public List<DetalleVentaVM> DetallesVenta { get; set; }
    public List<ModificarVentaPromocionVM> VentaPromociones { get; set; }
    public List<ListarProductosVM> Productos { get; set; }
    public List<ListarPromocionesVM> Promociones { get; set; }
    public List<ListarMetodosPagoVM> Metodos { get; set; }

    public ModificarVentaVM()
    {
        DetallesVenta = new List<DetalleVentaVM>();
        VentaPromociones = new List<ModificarVentaPromocionVM>();
        Productos = new List<ListarProductosVM>();
        Promociones = new List<ListarPromocionesVM>();
        Metodos = new List<ListarMetodosPagoVM>();
    }
    public ModificarVentaVM(List<ListarProductosVM> productos, List<ListarPromocionesVM> promociones, List<ListarMetodosPagoVM> metodos) : this()
    {
        Productos = productos;
        Promociones = promociones;
        Metodos = metodos;
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DetallesVenta.Count() == 0 && VentaPromociones.Count() == 0)
        {
            yield return new ValidationResult(
                "La venta debe contener al menos un producto o una promoción."
            );
        }
    }
}