namespace DetallesPromocionesVM;

public class ModificarDetallePromocionVM
{
    private int idPromocion;
    private int idProducto;
    private decimal cantidad;
    public int IdPromocion { get => idPromocion; set => IdPromocion = value; }
    public int IdProducto { get => idProducto; set => IdProducto = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value;}
}