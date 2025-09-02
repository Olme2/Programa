using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using DetallesComprasVM;
using ProductosVM;
using ProveedoresVM;

public class AltaCompraVM : IValidatableObject
{
    [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
    [Display(Name = "Proveedor")]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una fecha.")]
    [DataType(DataType.Date)]
    public DateOnly Fecha { get; set; }

    [StringLength(100, ErrorMessage = "El detalle no puede exceder los 100 caracteres.")]
    public string? Detalle { get; set; }

    public List<DetalleCompraVM> DetallesCompra { get; set; }
    public List<ListarProductosVM> Productos { get; set; }
    public List<ListarProveedoresVM> Proveedores { get; set; }

    public AltaCompraVM()
    {
        Fecha = DateOnly.FromDateTime(DateTime.Now);
        DetallesCompra = new List<DetalleCompraVM>();
        Productos = new List<ListarProductosVM>();
        Proveedores = new List<ListarProveedoresVM>();
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DetallesCompra.Count == 0)
        {
            yield return new ValidationResult("La compra debe contener al menos un producto."
            );
        }
        var productosDuplicados = DetallesCompra
            .GroupBy(d => d.IdProducto)
            .Any(g => g.Count() > 1);

        if (productosDuplicados)
        {
            yield return new ValidationResult("No se puede agregar el mismo producto más de una vez a la compra.");
        }
    }
}
