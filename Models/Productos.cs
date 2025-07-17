using ProductosVM;

public class Productos
{
    private int idProducto;
    private int idProveedor;
    private string producto;
    private decimal stock;
    private decimal costo;
    private decimal precio;

    public Productos()
    {
        producto = string.Empty;
    }

    public Productos(AltaProductoVM productoVM)
    {
        idProveedor = productoVM.IdProveedor;
        producto = productoVM.Producto;
        stock = productoVM.Stock;
        costo = productoVM.Costo;
        precio = productoVM.Precio;
    }

    public Productos(ModificarProductoVM productoVM)
    {
        idProducto = productoVM.IdProducto;
        idProveedor = productoVM.IdProveedor;
        producto = productoVM.Producto;
        stock = productoVM.Stock;
        costo = productoVM.Costo;
        precio = productoVM.Precio;
    }

    public int IdProducto { get => idProducto; set => idProducto = value; }
    public int IdProveedor { get => idProveedor; set => idProveedor = value; }
    public string Producto { get => producto; set => producto = value; }
    public decimal Stock { get => stock; set => stock = value; }
    public decimal Costo { get => costo; set => costo = value; }
    public decimal Precio { get => precio; set => precio = value; }
}