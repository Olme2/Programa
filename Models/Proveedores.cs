public class Proveedores
{
    private int idProveedor;
    private string proveedor;
    private string? contacto;

    public Proveedores()
    {
        proveedor = string.Empty;
    }

    public Proveedores(int IdProveedor, string Proveedor, string Contacto)
    {
        idProveedor = IdProveedor;
        proveedor = Proveedor;
        contacto = Contacto;
    }

    public Proveedores(AltaProveedorViewModel proveedorVM)
    {
        proveedor = proveedorVM.Proveedor;
        contacto = proveedorVM.Contacto;
    }

    public Proveedores(ModificarProveedorViewModel proveedorVM)
    {
        idProveedor = proveedorVM.IdProveedor;
        proveedor = proveedorVM.Proveedor;
        contacto = proveedorVM.Contacto;
    }

    public int IdProveedor { get => idProveedor; set => idProveedor = value; }
    public string Proveedor { get => proveedor; set => proveedor = value; }
    public string? Contacto { get => contacto; set => contacto = value; }
} 