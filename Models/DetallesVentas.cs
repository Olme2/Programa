public class DetallesVentas
{
    private long idVenta;
    private int idProducto;
    private decimal cantidad;
    private decimal costoUnitario;
    private decimal precioUnitario;
    
    public long IdVenta { get => idVenta; set => idVenta = value; }
    public int IdProducto { get => idProducto; set => idProducto = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value; }
    public decimal CostoUnitario { get => costoUnitario; set => costoUnitario = value; }
    public decimal PrecioUnitario { get => precioUnitario; set => precioUnitario = value; }
}
