using System.ComponentModel.DataAnnotations;
public class ModificarProductoViewModel
{
    private int idProducto;
    private string producto;
    private int idProveedor;
    private decimal stock;
    private decimal costo;
    private decimal precio;
    private List<ListarProveedoresViewModel> proveedores;

    public ModificarProductoViewModel()
    {
        producto = string.Empty;
        proveedores = new List<ListarProveedoresViewModel>();
    }
    public ModificarProductoViewModel(Productos Producto, List<ListarProveedoresViewModel> Proveedores)
    {
        idProducto = Producto.IdProducto;
        producto = Producto.Producto;
        idProveedor = Producto.IdProveedor;
        stock = Producto.Stock;
        costo = Producto.Costo;
        precio = Producto.Precio;
        proveedores = Proveedores;
    }

    public int IdProducto { get => idProducto; set => idProducto = value; }
    [Required(ErrorMessage = "Nombre obligatorio")]
    public string Producto { get => producto; set => producto = value; }
    [Required(ErrorMessage = "Proveedor obligatorio")]
    public int IdProveedor { get => idProveedor; set => idProveedor = value; }
    [Required(ErrorMessage = "Stock obligatorio")]
    public decimal Stock { get => stock; set => stock = value; }
    [Required(ErrorMessage = "Costo obligatorio")]
    public decimal Costo { get => costo; set => costo = value; }
    [Required(ErrorMessage = "Precio obligatorio")]
    public decimal Precio { get => precio; set => precio = value; }
    public List<ListarProveedoresViewModel> Proveedores { get => proveedores; set => proveedores = value; }
}