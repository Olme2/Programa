namespace ProveedoresVM;

public class ListarProveedoresVM
{
    private int idProveedor;
    private string proveedor;
    private string? contacto;

    public ListarProveedoresVM()
    {
        proveedor = string.Empty;
    }
    public ListarProveedoresVM(Proveedores Proveedor)
    {
        idProveedor = Proveedor.IdProveedor;
        proveedor = Proveedor.Proveedor;
        contacto = Proveedor.Contacto;
    }

    public int IdProveedor { get => idProveedor; set => idProveedor = value; }
    public string Proveedor { get => proveedor; set => proveedor = value; }
    public string? Contacto { get => contacto; set => contacto = value; }
}