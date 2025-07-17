using DetallesComprasVM;

public class DetallesCompras
{
    private int idCompra;
    private int idProducto;
    private decimal cantidad;
    private decimal precio;

    public DetallesCompras() { }
    public DetallesCompras(int IdCompra, int IdProducto, decimal Cantidad, decimal Precio)
    {
        idCompra = IdCompra;
        idProducto = IdProducto;
        cantidad = Cantidad;
        precio = Precio;
    }
    public DetallesCompras(AltaDetalleCompraVM detalleCompraVM)
    {
        idCompra = detalleCompraVM.IdCompra;
        idProducto = detalleCompraVM.IdProducto;
        cantidad = detalleCompraVM.Cantidad;
        precio = detalleCompraVM.Precio;
    }
    public DetallesCompras(ModificarDetalleCompraVM detalleCompraVM)
    {
        idCompra = detalleCompraVM.IdCompra;
        idProducto = detalleCompraVM.IdProducto;
        cantidad = detalleCompraVM.Cantidad;
        precio = detalleCompraVM.Precio;
    }
    public DetallesCompras(ListarDetallesCompraVM detalleCompraVM)
    {
        idCompra = detalleCompraVM.IdCompra;
        idProducto = detalleCompraVM.IdProducto;
        cantidad = detalleCompraVM.Cantidad;
        precio = detalleCompraVM.Precio;
    }
    public int IdCompra { get => idCompra; set => idCompra = value; }
    public int IdProducto { get => idProducto; set => idProducto = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value; }
    public decimal Precio { get => precio; set => precio = value; }
}