using System.ComponentModel.DataAnnotations;
namespace ProveedoresVM;

public class AltaProveedorVM
{
    private string proveedor;
    private string? contacto;

    public AltaProveedorVM()
    {
        proveedor = string.Empty;
        contacto = string.Empty;
    }

    [Required(ErrorMessage = "Nombre del proveedor obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede ser más largo que 50 caracteres")]
    public string Proveedor { get => proveedor; set => proveedor = value; }

    [StringLength(100, ErrorMessage = "El contacto no puede ser más largo que 100 caracteres")]
    public string? Contacto { get => contacto; set => contacto = value; }
}