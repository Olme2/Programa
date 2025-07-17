using Microsoft.AspNetCore.Mvc.Rendering;
namespace DetallesVentasVM;

public class ModificarDetalleVentaVM
{
    private long idVenta;
    private int idProducto;
    private List<SelectListItem> productos;
    private decimal cantidad;
    private bool promocion;
    private decimal precioPromo;
    private decimal costoPromo;
    public long IdVenta { get => idVenta; set => idVenta = value; }
    public int IdProducto { get => idProducto; set => idProducto = value; }
    public List<SelectListItem> Productos { get => productos; set => productos = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value; }
    public bool Promocion { get => promocion; set => promocion = value; }
    public decimal PrecioPromo { get => precioPromo; set => precioPromo = value; }
    public decimal CostoPromo { get => costoPromo; set => costoPromo = value; }
}