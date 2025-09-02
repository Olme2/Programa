using ComprasVM;
using DetallesComprasVM;
public class DetallesCompras
{
    public int IdCompra { get; set; }
    public int IdProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public virtual Productos Producto { get; set; } = null!;
    public virtual Compras Compra { get; set; } = null!;
    private DetallesCompras() { }
    public DetallesCompras(int idProducto, decimal cantidad, decimal costoUnitario)
    {
        IdProducto = idProducto;
        Cantidad = cantidad;
        CostoUnitario = costoUnitario;
    }

    public static DetallesCompras CrearDesdeViewModel(DetalleCompraVM detalleVM)
    {
        return new DetallesCompras(detalleVM.IdProducto, detalleVM.Cantidad, detalleVM.CostoUnitario);
    }

    public decimal CalcularTotal()
    {
        return CostoUnitario * Cantidad;
    }
}