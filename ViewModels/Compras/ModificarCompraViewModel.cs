namespace ComprasVM;

public class ModificarCompraVM
{
    private int idCompra;
    private DateOnly fecha;
    private string? detalle;
    public int IdCompra { get => idCompra; set => idCompra = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
}