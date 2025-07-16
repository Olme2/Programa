public class DetallesVentas
{
    private long idVenta;
    private int idProducto;
    private decimal cantidad;
    private bool promocion;
    private decimal precioPromo;
    private decimal costoPromo;

    public long IdVenta { get => idVenta; set => idVenta = value; }
    public int IdProducto { get => idProducto; set => idProducto = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value; }
    public bool Promocion { get => promocion; set => promocion = value; }
    public decimal PrecioPromo { get => precioPromo; set => precioPromo = value; }
    public decimal CostoPromo { get => costoPromo; set => costoPromo = value; }
    public Productos? Producto { get; set; }
    public Ventas? Venta { get; set; }

    public DetallesVentas() {}
    public DetallesVentas(long idVenta, int idProducto, decimal cantidad, bool promocion, decimal precioPromo, decimal costoPromo)
    {
        IdVenta = idVenta;
        IdProducto = idProducto;
        Cantidad = cantidad;
        Promocion = promocion;
        PrecioPromo = precioPromo;
        CostoPromo = costoPromo;
    }
}
