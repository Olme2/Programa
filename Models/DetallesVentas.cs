using DetallesVentasVM;

public class DetallesVentas
{
    public long IdVenta { get; set; }
    public int IdProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal PrecioUnitario { get; set; }
    public virtual Productos Producto { get; set; }
    private DetallesVentas()
    {
        Producto = null!;
    }

    private DetallesVentas(int idProducto, decimal cantidad, decimal costoUnitario, decimal precioUnitario)
    {
        IdProducto = idProducto;
        Cantidad = cantidad;
        CostoUnitario = costoUnitario;
        PrecioUnitario = precioUnitario;
        Producto = null!;
    }

    public static DetallesVentas CrearDesdeViewModel(AltaDetalleVentaVM detalleVM)
    {
        return new DetallesVentas(detalleVM.IdProducto, detalleVM.Cantidad, detalleVM.CostoUnitario, detalleVM.PrecioUnitario);
    }

    public static DetallesVentas CrearDesdeViewModel(ModificarDetalleVentaVM detalleVM)
    {
        return new DetallesVentas(detalleVM.IdProducto, detalleVM.Cantidad, detalleVM.CostoUnitario, detalleVM.PrecioUnitario);
    }

    public decimal CalcularCosto()
    {
        return CostoUnitario * Cantidad;
    }

    public decimal CalcularPrecio()
    {
        return PrecioUnitario * Cantidad;
    }
}