using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using DetallesVentasVM;
using ProductosVM;
using PromocionesVM;
using VentasPromocionesVM;

namespace VentasVM;

public class AltaVentaVM : IValidatableObject
{
    [Required(ErrorMessage = "Debe seleccionar un método de pago.")]
    [Display(Name = "Método de Pago")]
    public short IdMetodo { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una fecha.")]
    public DateOnly Fecha { get; set; }

    [Required]
    public TimeOnly Hora { get; set; }
    [Display(Name = "Recargo (%)")]
    [Range(0.000, 100.000, ErrorMessage = "El recargo no puede ser negativo ni mayor al 100%.")]
    public decimal Recargo { get; set; } = 0; // Campo para el recargo, con valor por defecto 0.

    public string? Detalle { get; set; }

    public List<DetalleVentaVM> DetallesVenta { get; set; }
    public List<VentaPromocionVM> VentaPromociones { get; set; }
    
    // Propiedades para rellenar los selectores del formulario
    public List<ListarProductosVM> Productos { get; set; }
    public List<ListarPromocionesVM> Promociones { get; set; }
    public List<SelectListItem> MetodosPago { get; set; } // Cambiado para que sea más fácil de usar en la vista.

    public AltaVentaVM()
    {
        Fecha = DateOnly.FromDateTime(DateTime.Now);
        Hora = TimeOnly.FromDateTime(DateTime.Now);
        DetallesVenta = new List<DetalleVentaVM>();
        VentaPromociones = new List<VentaPromocionVM>();
        Productos = new List<ListarProductosVM>();
        Promociones = new List<ListarPromocionesVM>();
        MetodosPago = new List<SelectListItem>();
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DetallesVenta.Count == 0 && VentaPromociones.Count == 0)
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