using DetallesVentasVM;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    [StringLength(100, ErrorMessage = "El detalle no puede tener mas de 100 caracteres")]
    public string? Detalle { get; set; }
    public decimal Recargo { get; set; } = 0;
    public List<DetalleVentaVM> DetallesVenta { get; set; }
    public List<VentaPromocionVM> VentaPromociones { get; set; }
    public List<SelectListItem> Metodos { get; set; }

    public ModificarVentaVM()
    {
        DetallesVenta = new List<DetalleVentaVM>();
        VentaPromociones = new List<VentaPromocionVM>();
        Metodos = new List<SelectListItem>();
    }
    public ModificarVentaVM(Ventas venta, List<SelectListItem> metodos)
    {
        IdVenta = venta.IdVenta;
        IdMetodo = venta.IdMetodo;
        Fecha = venta.Fecha;
        Hora = venta.Hora;
        Detalle = venta.Detalle;
        Metodos = metodos;
        DetallesVenta = venta.DetallesVenta.Select(d => new DetalleVentaVM(d)).ToList();
        VentaPromociones = venta.VentaPromociones.Select(p => new VentaPromocionVM(p)).ToList();
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DetallesVenta.Count() == 0 && VentaPromociones.Count() == 0)
        {
            yield return new ValidationResult(
                "La venta debe contener al menos un producto o una promoción."
            );
        }
        var productosDuplicados = DetallesVenta
        .GroupBy(d => d.IdProducto)
        .Any(g => g.Count() > 1);

        if (productosDuplicados)
        {
            yield return new ValidationResult("No se puede agregar el mismo producto más de una vez a la venta.");
        }

        // Regla 3: No puede haber promociones duplicadas.
        var promocionesDuplicadas = VentaPromociones
            .GroupBy(vp => vp.IdPromocion)
            .Any(g => g.Count() > 1);

        if (promocionesDuplicadas)
        {
            yield return new ValidationResult("No se puede agregar la misma promoción más de una vez a la venta.");
        }
    }
}