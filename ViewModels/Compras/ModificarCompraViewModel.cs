using System.ComponentModel.DataAnnotations;
using DetallesComprasVM;
using ComprasVM;
using ProveedoresVM;

public class ModificarCompraVM : IValidatableObject
{
    [Required]
    public int IdCompra { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
    [Display(Name = "Proveedor")]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una fecha.")]
    [DataType(DataType.Date)]
    public DateOnly Fecha { get; set; }

    [StringLength(100, ErrorMessage = "El detalle no puede exceder los 100 caracteres.")]
    public string? Detalle { get; set; }

    [Display(Name = "Ya fue pagada")]
    public bool Pagada { get; set; }

    public List<DetalleCompraVM> DetallesCompra { get; set; }

    public List<ListarProveedoresVM> Proveedores { get; set; }

    public ModificarCompraVM()
    {
        DetallesCompra = new List<DetalleCompraVM>();
        Proveedores = new List<ListarProveedoresVM>();
    }

    public ModificarCompraVM(Compras compra, List<ListarProveedoresVM> proveedores)
    {
        IdCompra = compra.IdCompra;
        IdProveedor = compra.IdProveedor;
        Fecha = compra.Fecha;
        Detalle = compra.Detalle;
        Pagada = compra.Pagada;
        Proveedores = proveedores;
        DetallesCompra = compra.DetallesCompra.Select(d => new DetalleCompraVM(d)).ToList();
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DetallesCompra.Count == 0)
        {
            yield return new ValidationResult("La compra debe contener al menos un producto.", new[] { nameof(DetallesCompra) });
        }
        var productosDuplicados = DetallesCompra
        .GroupBy(d => d.IdProducto)
        .Any(g => g.Count() > 1);

        if (productosDuplicados)
        {
            yield return new ValidationResult("No se puede agregar el mismo producto más de una vez a la venta.");
        }
    }
}
