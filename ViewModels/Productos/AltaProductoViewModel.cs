using System.ComponentModel.DataAnnotations;
namespace ProductosVM;
using ProveedoresVM;

public class AltaProductoVM
{
    private string producto;
    private int idProveedor;
    private decimal stock;
    private decimal costo;
    private decimal precio;
    private List<ListarProveedoresVM> proveedores;
    public AltaProductoVM()
    {
        producto = string.Empty;
        proveedores = new List<ListarProveedoresVM>();
    }
    public AltaProductoVM(List<ListarProveedoresVM> Proveedores)
    {
        producto = string.Empty;
        proveedores = Proveedores;
    }
    [Required(ErrorMessage = "Nombre de producto Obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede ser más largo que 50 caracteres")]
    public string Producto { get => producto; set => producto = value; }
    [Required(ErrorMessage = "Id proveedor obligatorio")]
    public int IdProveedor { get => idProveedor; set => idProveedor = value; }
    [Required(ErrorMessage = "Stock Obligatorio")]
    public decimal Stock { get => stock; set => stock = value; }
    [Required(ErrorMessage = "Costo Obligatorio")]
    public decimal Costo { get => costo; set => costo = value; }
    [Required(ErrorMessage = "Precio Obligatorio")]
    public decimal Precio { get => precio; set => precio = value; }
    public List<ListarProveedoresVM> Proveedores { get => proveedores;  set => proveedores = value; }
}