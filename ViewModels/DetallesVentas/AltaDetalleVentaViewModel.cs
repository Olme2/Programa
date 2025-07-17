using Microsoft.AspNetCore.Mvc.Rendering;
using ProductosVM;
namespace DetallesVentasVM;

public class AltaDetalleVentaVM
{
    private long idVenta;
    private int idProducto;
    private decimal cantidad;
    private bool promocion;
    private decimal precioPromo;
    private decimal costoPromo;
    private List<ListarProductosVM> productos;
    public long IdVenta { get => idVenta; set => idVenta = value; }
    public int IdProducto { get => idProducto; set => idProducto = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value; }
    public bool Promocion { get => promocion; set => promocion = value; }
    public decimal PrecioPromo { get => precioPromo; set => precioPromo = value; }
    public decimal CostoPromo { get => costoPromo; set => costoPromo = value; }
    public List<ListarProductosVM> Productos { get => productos; set => productos = value; }
}