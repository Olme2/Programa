public class Productos
{
    private int idProducto;
    private int idProveedor;
    private string producto;
    private decimal stock;
    private decimal costo;
    private decimal precio;
    private decimal ganancia;
    private decimal porcentajeGanancia;
    public Productos()
    {
        producto = string.Empty;
    }
    public Productos(int IdProducto, int IdProveedor, string Producto, decimal Stock, decimal Costo, decimal Precio, decimal Ganancia, decimal PorcentajeGanancia)
    {
        idProducto = IdProducto;
        idProveedor = IdProveedor;
        producto = Producto;
        stock = Stock;
        costo = Costo;
        precio = Precio;
        ganancia = Ganancia;
        porcentajeGanancia = PorcentajeGanancia;
    }
    public Productos(AltaProductoViewModel productoVM)
    {
        idProveedor = productoVM.IdProveedor;
        producto = productoVM.Producto;
        stock = productoVM.Stock;
        costo = productoVM.Costo;
        precio = productoVM.Precio;
        // No setear Ganancia ni PorcentajeGanancia
    }

    public Productos(ModificarProductoViewModel productoVM)
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
    public decimal Ganancia { get => ganancia; set => ganancia = value; }
    public decimal PorcentajeGanancia { get => porcentajeGanancia; set => porcentajeGanancia = value; }
}