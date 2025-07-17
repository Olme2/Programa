using ComprasVM;

public class Compras
{
    private int idCompra;
    private decimal total;
    private DateOnly fecha;
    private string? detalle;

    public Compras()
    {
        total = 0;
        fecha = DateOnly.FromDateTime(DateTime.Now);
    }
    public Compras(int IdCompra, decimal Total, DateOnly Fecha, string? Detalle)
    {
        idCompra = IdCompra;
        total = Total;
        fecha = Fecha;
        detalle = Detalle;
    }
    public Compras(AltaCompraVM compraVM)
    {
        total = compraVM.Total;
        fecha = compraVM.Fecha;
        detalle = compraVM.Detalle;
    }
    public Compras(ModificarCompraVM compraVM)
    {
        idCompra = compraVM.IdCompra;
        fecha = compraVM.Fecha;
        detalle = compraVM.Detalle;
    }
    public Compras(ListarComprasVM compraVM)
    {
        idCompra = compraVM.IdCompra;
        total = compraVM.Total;
        fecha = compraVM.Fecha;
        detalle = compraVM.Detalle;
    }

    public int IdCompra { get => idCompra; set => idCompra = value; }
    public decimal Total { get => total; set => total = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
}