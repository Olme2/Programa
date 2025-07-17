namespace DetallesVentasVM;

public class ListarDetallesVentaVM
{
    private long idVenta;
    private int idProducto;
    private string producto;
    private string proveedor;
    private decimal cantidad;
    private bool promocion;
    private decimal precioPromo;
    private decimal costoPromo;
    private decimal precio;

    public ListarDetallesVentaVM()
    {
        producto = string.Empty;
        proveedor = string.Empty;
    }

    public long IdVenta { get => idVenta; set => idVenta = value; }
    public int IdProducto { get => idProducto; set => idProducto = value; }
    public string Producto { get => producto; set => producto = value; }
    public string Proveedor { get => proveedor; set => proveedor = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value; }
    public bool Promocion { get => promocion; set => promocion = value; }
    public decimal PrecioPromo { get => precioPromo; set => precioPromo = value; }
    public decimal CostoPromo { get => costoPromo; set => costoPromo = value; }
}