namespace ProductosVM;

public class ListarProductosVM
{
    private int idProducto;
    private string producto;
    private string proveedor;
    private decimal stock;
    private decimal costo;
    private decimal precio;
    private decimal ganancia;
    private decimal porcentajeGanancia;
    public ListarProductosVM()
    {
        producto = string.Empty;
        proveedor = string.Empty;
    }
    public ListarProductosVM(Productos Producto, string Proveedor)
    {
        idProducto = Producto.IdProducto;
        producto = Producto.Producto;
        proveedor = Proveedor;
        stock = Producto.Stock;
        costo = Producto.Costo;
        precio = Producto.Precio;
        ganancia = Producto.Ganancia;
        porcentajeGanancia = Producto.PorcentajeGanancia;
    }
    public int IdProducto { get => idProducto; set => idProducto = value; }
    public string Producto { get => producto; set => producto = value; }
    public string Proveedor { get => proveedor; set => proveedor = value; }
    public decimal Stock { get => stock; set => stock = value; }
    public decimal Costo { get => costo; set => costo = value; }
    public decimal Precio { get => precio; set => precio = value; }
    public decimal Ganancia { get => ganancia; set => ganancia = value; }
    public decimal PorcentajeGanancia { get => porcentajeGanancia; set => porcentajeGanancia = value; }
}