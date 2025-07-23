using System.ComponentModel.DataAnnotations;
using DetallesVentasVM;
using MetodosVM;
using ProductosVM;
using PromocionesVM;
using VentasPromocionesVM;
namespace VentasVM;

public class AltaVentaVM : IValidatableObject
{
    [Required(ErrorMessage = "Debe seleccionar un método de pago.")]
    [Display(Name = "Método de Pago")]
    public short IdMetodo { get; set; }
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly Fecha { get; set; }
    [Required(ErrorMessage = "La hora es obligatoria.")]
    public TimeOnly Hora { get; set; }
    public string? Detalle { get; set; }
    public List<AltaDetalleVentaVM> DetallesVenta { get; set; }
    public List<AltaVentaPromocionVM> VentaPromociones { get; set; }
    public List<ListarProductosVM> Productos { get; set; }
    public List<ListarPromocionesVM> Promociones { get; set; }
    public List<ListarMetodosDePagoVM> Metodos { get; set; }

    public AltaVentaVM()
    {
        Fecha = DateOnly.FromDateTime(DateTime.Now);
        Hora = TimeOnly.FromDateTime(DateTime.Now);
        DetallesVenta = new List<AltaDetalleVentaVM>();
        VentaPromociones = new List<AltaVentaPromocionVM>();
        Productos = new List<ListarProductosVM>();
        Promociones = new List<ListarPromocionesVM>();
        Metodos = new List<ListarMetodosDePagoVM>();
    }
    public AltaVentaVM(List<ListarProductosVM> productos, List<ListarPromocionesVM> promociones, List<ListarMetodosDePagoVM> metodos) : this()
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