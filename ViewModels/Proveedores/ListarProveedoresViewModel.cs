using System.ComponentModel.DataAnnotations;
namespace ProveedoresVM;

public class ListarProveedoresVM
{
    public int IdProveedor { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    [DataType(DataType.Currency)] 
    public decimal Debo { get; set; } 
    public bool EsEliminable { get; set; }
    public ListarProveedoresVM() { }

    public ListarProveedoresVM(Proveedores proveedor)
    {
        IdProveedor = proveedor.IdProveedor;
        Proveedor = proveedor.Proveedor;
        Contacto = proveedor.Contacto;
        Debo = proveedor.Debo;
        EsEliminable = false;
    }
}