using System.ComponentModel.DataAnnotations;
namespace ProveedoresVM;
public class ModificarProveedorVM
{
    [Required]
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [StringLength(75, ErrorMessage = "El nombre no puede exceder los 75 caracteres.")]
    public string Proveedor { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El contacto no puede exceder los 100 caracteres.")]
    public string? Contacto { get; set; }
    [Required(ErrorMessage = "El saldo al proveedor es obligatorio.")]
    [Range(0, 9999999.99, ErrorMessage = "El saldo no puede ser negativo.")]
    public decimal Debo { get; set; }
    public ModificarProveedorVM() { }

    public ModificarProveedorVM(Proveedores proveedor)
    {
        IdProveedor = proveedor.IdProveedor;
        Proveedor = proveedor.Proveedor;
        Contacto = proveedor.Contacto;
        Debo = proveedor.Debo;
    }
}