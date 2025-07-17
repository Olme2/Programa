using DetallesComprasVM;

public class DetallesCompras
{
    private int idCompra;
    private int idProducto;
    private decimal cantidad;
    private decimal costoUnitario;

    public DetallesCompras(){}

    public DetallesCompras(AltaDetalleCompraVM detalleCompraVM)
    {
        idCompra = detalleCompraVM.IdCompra;
        idProducto = detalleCompraVM.IdProducto;
        cantidad = detalleCompraVM.Cantidad;
        costoUnitario = detalleCompraVM.CostoUnitario;
    }

    public DetallesCompras(ModificarDetalleCompraVM detalleCompraVM)
    {
        idCompra = detalleCompraVM.IdCompra;
        idProducto = detalleCompraVM.IdProducto;
        cantidad = detalleCompraVM.Cantidad;
        costoUnitario = detalleCompraVM.CostoUnitario;
    }

    public int IdCompra { get => idCompra; set => idCompra = value; }
    public int IdProducto { get => idProducto; set => idProducto = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value; }
    public decimal CostoUnitario { get => costoUnitario; set => costoUnitario = value; }
}