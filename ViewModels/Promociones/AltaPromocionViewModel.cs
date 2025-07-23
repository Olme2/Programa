namespace PromocionesVM;
using System.ComponentModel.DataAnnotations;
using DetallesPromocionesVM;
using ProductosVM;

public class AltaPromocionVM : IValidatableObject
{
    [Required(ErrorMessage = "El nombre de la promoción es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Promocion { get; set; }
    [Required(ErrorMessage = "El precio de la promoción es obligatorio.")]
    [Range(0.01, 99999.99, ErrorMessage = "El precio debe estar entre 0,01 y 99.999,99.")]
    public decimal Precio { get; set; }
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly Inicio { get; set; }
    public DateOnly? Fin { get; set; }
    [MinLength(1, ErrorMessage = "La promoción debe tener al menos un producto.")]
    public List<AltaDetallePromocionVM> DetallesPromocion { get; set; }
    public List<ListarProductosVM> Productos { get; set; }

    public AltaPromocionVM()
    {
        Promocion = string.Empty;
        Inicio = DateOnly.FromDateTime(DateTime.Now);
        DetallesPromocion = new List<AltaDetallePromocionVM>();
        Productos = new List<ListarProductosVM>();
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Lógica para verificar duplicados
        var tieneDuplicados = DetallesPromocion.GroupBy(d => d.IdProducto).Any(g => g.Count() > 1);
        if (tieneDuplicados)
        {
            yield return new ValidationResult(
                "No se puede agregar el mismo producto más de una vez a la promoción."
            );
        }
        if (Inicio > Fin)
        {
            yield return new ValidationResult(
                "La fecha de inicio no puede ser mayor que la de fin.",
                [nameof(Inicio), nameof(Fin)]
            );
        }
    }
}