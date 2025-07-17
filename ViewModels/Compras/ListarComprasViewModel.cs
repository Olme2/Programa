namespace ComprasVM;

public class ListarComprasVM
{
    private int idCompra;
    private string proveedor;
    private decimal total;
    private DateOnly fecha;
    private string? detalle;
    public int IdCompra { get => idCompra; set => idCompra = value; }
    public string Proveedor { get => proveedor;  set => proveedor = value; }
    public decimal Total { get => total; set => total = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
}