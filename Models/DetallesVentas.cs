public class DetallesVentas
{
    // --- Propiedades de la Entidad ---
    public long IdVenta { get; set; }
    public int IdProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal PrecioUnitario { get; set; }
    public virtual Ventas Venta { get; set; }
    public virtual Productos Producto { get; set; }
    private DetallesVentas()
    {
        Venta = null!;
        Producto = null!;
    }

}