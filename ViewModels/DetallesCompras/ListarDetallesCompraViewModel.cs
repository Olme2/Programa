namespace DetallesComprasVM;

public class ListarDetallesCompraVM
{
    private int idCompra;
    private int idProducto;
    private decimal cantidad;
    private decimal precio;
    public int IdCompra { get => idCompra; set => idCompra = value; }
    public int IdProducto { get => idProducto;  set => idProducto = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value;}
    public decimal Precio { get => precio; set => precio = value; }
}