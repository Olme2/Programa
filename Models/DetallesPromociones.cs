using DetallesPromocionesVM;

public class DetallesPromociones
{
    private int idPromocion;
    private int idProducto;
    private decimal cantidad;

    public DetallesPromociones(){}
    public DetallesPromociones(int IdPromocion, int IdProducto, decimal Cantidad)
    {
        idPromocion = IdPromocion;
        idProducto = IdProducto;
        cantidad = Cantidad;
    }
    public DetallesPromociones(AltaDetallePromocionVM detallePromocionVM)
    {
        idPromocion = detallePromocionVM.IdPromocion;
        idProducto = detallePromocionVM.IdProducto;
        cantidad = detallePromocionVM.Cantidad;
    }
    public DetallesPromociones(ModificarDetallePromocionVM detallePromocionVM)
    {
        idPromocion = detallePromocionVM.IdPromocion;
        idProducto = detallePromocionVM.IdProducto;
        cantidad = detallePromocionVM.Cantidad;
    }
    public DetallesPromociones(ListarDetallesPromocionVM detallePromocionVM)
    {
        idPromocion = detallePromocionVM.IdPromocion;
        idProducto = detallePromocionVM.IdProducto;
        cantidad = detallePromocionVM.Cantidad;
    }

    public int IdPromocion { get => idPromocion; set => idPromocion = value; }
    public int IdProducto { get => idProducto; set => idProducto = value; }
    public decimal Cantidad { get => cantidad; set => cantidad = value; }
}
