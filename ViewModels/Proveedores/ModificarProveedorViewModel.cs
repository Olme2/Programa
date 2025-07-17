using System.ComponentModel.DataAnnotations;

public class ModificarProveedorVM
{
    private int idProveedor;
    private string proveedor;
    private string? contacto;

    public ModificarProveedorVM()
    {
        proveedor = string.Empty;
        contacto = string.Empty;
    }

    public ModificarProveedorVM(Proveedores Proveedor)
    {
        idProveedor = Proveedor.IdProveedor;
        proveedor = Proveedor.Proveedor;
        contacto = Proveedor.Contacto;
    }

    public int IdProveedor { get => idProveedor; set => idProveedor = value; }

    [Required(ErrorMessage = "Nombre del proveedor obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede ser más largo que 50 caracteres")]
    public string Proveedor { get => proveedor; set => proveedor = value; }
    [StringLength(100, ErrorMessage = "El contacto no puede ser más largo que 100 caracteres")]
    public string? Contacto { get => contacto; set => contacto = value; }
} 