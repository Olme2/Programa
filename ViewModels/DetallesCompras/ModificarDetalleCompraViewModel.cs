namespace DetallesComprasVM;

public class ModificarDetalleCompraVM
{
    private int idCompra;
    private int idProducto;
    private decimal cantidad;
    private decimal costoUnitario;
    public int IdCompra { get => idCompra; set => idCompra = value; }
    public int IdProducto { get => idProducto;  set => idProducto = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value;}
    public decimal CostoUnitario { get => costoUnitario; set => costoUnitario = value; }
}