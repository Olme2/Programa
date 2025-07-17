public class Ventas
{
    private long idVenta;
    private short idMetodo;
    private decimal total;
    private decimal costo;
    private decimal ganancia;
    private decimal porcentajeGanancia;
    private DateOnly fecha;
    private TimeOnly hora;
    private string? detalle;
    public long IdVenta { get => idVenta; set => idVenta = value; }
    public short IdMetodo { get => idMetodo; set => idMetodo = value; }
    public decimal Total { get => total; set => total = value; }
    public decimal Costo { get => costo; set => costo = value; }
    public decimal Ganancia { get => ganancia; set => ganancia = value; }
    public decimal PorcentajeGanancia { get => porcentajeGanancia; set => porcentajeGanancia = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public TimeOnly Hora { get => hora; set => hora = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
}

