using System.ComponentModel.DataAnnotations;
using DetallesPromocionesVM; 
using ProductosVM;

namespace PromocionesVM;

public class ModificarPromocionVM : IValidatableObject
{
    [Required]
    public int IdPromocion { get; set; }
    [Required(ErrorMessage = "El nombre de la promoción es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Promocion { get; set; }
    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 99999.99, ErrorMessage = "El precio debe estar entre 0,01 y 99.999,99.")]
    public decimal Precio { get; set; }
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly Inicio { get; set; }
    public DateOnly? Fin { get; set; }
    [MinLength(1, ErrorMessage = "La promoción debe tener al menos un producto.")]
    public List<DetallePromocionVM> DetallesPromocion { get; set; }
    // Propiedad para la lista de TODOS los productos disponibles.
    public bool EsEliminable { get; set; }
    public bool EsModificable { get; set; }
    public List<ListarProductosVM> Productos { get; set; }
    public ModificarPromocionVM()
    {
        Promocion = string.Empty;
        DetallesPromocion = new List<DetallePromocionVM>();
        Productos = new List<ListarProductosVM>();
    }
    // Constructor que mapea la entidad al ViewModel.
    public ModificarPromocionVM(Promociones promocion) : this()
    {
        IdPromocion = promocion.IdPromocion;
        Promocion = promocion.Promocion;
        Precio = promocion.Precio;
        Inicio = promocion.Inicio;
        Fin = promocion.Fin;
        DetallesPromocion = promocion.DetallesPromocion.Select(d => new DetallePromocionVM(d)).ToList();
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
    public decimal CalcularCosto()
    {
        return DetallesPromocion.Sum(d => d.CalcularCosto());
    }
    public decimal CalcularGanancia()
    {
        return Precio - CalcularCosto();
    }
    public decimal CalcularPorcentajeGanancia()
    {
        return CalcularCosto() > 0 ? 100*CalcularGanancia()/CalcularCosto() : 0;
    }
}