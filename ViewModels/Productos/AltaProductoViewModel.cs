using System.ComponentModel.DataAnnotations;
namespace ProductosVM;
using ProveedoresVM;

public class AltaProductoVM : IValidatableObject
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(75, ErrorMessage = "El nombre no puede exceder los 75 caracteres.")]
    public string Producto { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
    [Display(Name = "Proveedor")]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "El stock inicial es obligatorio.")]
    [Range(0, 9999.999, ErrorMessage = "El stock no puede ser negativo ni mayor a 9.999,999.")]
    public decimal Stock { get; set; } = -1;

    [Required(ErrorMessage = "El costo es obligatorio.")]
    [Range(0.01, 99999.99, ErrorMessage = "El costo debe ser un valor positivo y no mayor a $99.999,99.")]
    public decimal Costo { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 99999.99, ErrorMessage = "El precio debe ser un valor positivo y no mayor a $99.999,99.")]
    public decimal Precio { get; set; }
    public string? Proveedor {get; set;}
    public List<ListarProveedoresVM> Proveedores { get; set; }

    public AltaProductoVM()
    {
        Producto = string.Empty;
        Proveedores = new List<ListarProveedoresVM>();
    }

    public AltaProductoVM(List<ListarProveedoresVM> proveedores) : this()
    {
        Proveedores = proveedores;
    }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Precio <= Costo)
        {
            yield return new ValidationResult(
                "El precio no puede ser menor que el costo.",
                [nameof(Precio)]
            );
        }
    }
}
