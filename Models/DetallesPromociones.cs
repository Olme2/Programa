using DetallesPromocionesVM;

public class DetallesPromociones
{
    public int IdProducto { get; set; }
    public int IdPromocion { get; set; }
    public decimal Cantidad { get; set; }
    public virtual Productos Producto { get; set; }
    public DetallesPromociones()
    {
        Producto = null!;
    }
    public DetallesPromociones(int idProducto, decimal cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor que cero.", nameof(cantidad));
        }
        IdProducto = idProducto;
        Cantidad = cantidad;
        Producto = null!;
    }
    public static DetallesPromociones CrearDesdeViewModel(AltaDetallePromocionVM detalleVM)
    {
        return new DetallesPromociones(detalleVM.IdProducto, detalleVM.Cantidad);
    }
    public static DetallesPromociones CrearDesdeViewModel(ModificarDetallePromocionVM detalleVM)
    {
        return new DetallesPromociones(detalleVM.IdProducto, detalleVM.Cantidad);
    }
    public decimal CalcularCosto()
    {
        if (Producto == null)
        {
            throw new InvalidOperationException("El producto no fue cargado para calcular el costo.");
        }
        return Producto.Precio * Cantidad;
    }
}