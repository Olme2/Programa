using ComprasVM;

public class Compras
{
    private int idCompra;
    private int idProveedor;
    private decimal total;
    private DateOnly fecha;
    private string? detalle;

    public Compras()
    {
        total = 0;
        fecha = DateOnly.FromDateTime(DateTime.Now);
    }
    
    public Compras(AltaCompraVM compraVM)
    {
        idProveedor = compraVM.IdProveedor;
        total = compraVM.Total;
        fecha = compraVM.Fecha;
        detalle = compraVM.Detalle;
    }

    public Compras(ModificarCompraVM compraVM)
    {
        idCompra = compraVM.IdCompra;
        idProveedor = compraVM.IdProveedor;
        fecha = compraVM.Fecha;
        detalle = compraVM.Detalle;
    }

    public int IdCompra { get => idCompra; set => idCompra = value; }
    public int IdProveedor { get => idProveedor; set => idProveedor = value;}
    public decimal Total { get => total; set => total = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
}