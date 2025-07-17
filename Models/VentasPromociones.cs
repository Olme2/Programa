public class VentasPromociones
{
    private long idVenta;
    private int idPromocion;
    private short cantidad;
    private decimal costoPromo;
    private decimal precioPromo;
    public long IdVenta { get => idVenta; set => idVenta = value; }
    public int IdPromocion { get => idPromocion; set => idPromocion = value; }
    public short Cantidad { get => cantidad;  set => cantidad = value; }
    public decimal CostoPromo { get => costoPromo;  set => costoPromo = value; }
    public decimal PrecioPromo { get => precioPromo; set => precioPromo = value; }
}