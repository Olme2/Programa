using DetallesPromocionesVM;

public class DetallesPromociones
{
    public int IdProducto { get; set; }
    public int IdPromocion { get; set; }
    public decimal Cantidad { get; set; }
    public virtual Productos Producto { get; set; }
    private DetallesPromociones()
    {
        Producto = null!;
    }
    private DetallesPromociones(int idProducto, decimal cantidad)
    {
        IdProducto = idProducto;
        Cantidad = cantidad;
        Producto = null!;
    }
    public static DetallesPromociones CrearDesdeViewModel(DetallePromocionVM detalleVM)
    {
        return new DetallesPromociones(detalleVM.IdProducto, detalleVM.Cantidad);
    }
    public decimal CalcularCosto()
    {
        if (Producto == null)
        {
            throw new InvalidOperationException("El producto no fue cargado para calcular el costo.");
        }
        return Producto.Costo * Cantidad;
    }
}