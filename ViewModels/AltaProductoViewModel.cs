using System.ComponentModel.DataAnnotations;
public class AltaProductoViewModel
{
    private string producto;
    private int idProveedor;
    private decimal stock;
    private decimal costo;
    private decimal precio;
    private List<ListarProveedoresViewModel> proveedores;
    public AltaProductoViewModel()
    {
        producto = string.Empty;
        proveedores = new List<ListarProveedoresViewModel>();
    }
    public AltaProductoViewModel(List<ListarProveedoresViewModel> Proveedores)
    {
        producto = string.Empty;
        proveedores = Proveedores;
    }
    [Required(ErrorMessage = "Nombre Obligatorio")]
    public string Producto { get => producto; set => producto = value; }
    [Required(ErrorMessage = "Id proveedor obligatorio")]
    public int IdProveedor { get => idProveedor; set => idProveedor = value; }
    [Required(ErrorMessage = "Stock Obligatorio")]
    public decimal Stock { get => stock; set => stock = value; }
    [Required(ErrorMessage = "Costo Obligatorio")]
    public decimal Costo { get => costo; set => costo = value; }
    [Required(ErrorMessage = "Precio Obligatorio")]
    public decimal Precio { get => precio; set => precio = value; }
    public List<ListarProveedoresViewModel> Proveedores { get => proveedores;  set => proveedores = value; }
}