namespace VentasVM;

public class ListarVentasVM
{
    private long idVenta;
    private decimal total;
    private DateOnly fecha;
    private TimeOnly hora;
    private string metodoPagoNombre;
    private decimal costo;
    private decimal ganancia;
    private decimal porcentajeGanancia;
    public long IdVenta { get => idVenta; set => idVenta = value; }
    public decimal Total { get => total; set => total = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public TimeOnly Hora { get => hora; set => hora = value; }
    public string MetodoPagoNombre { get => metodoPagoNombre; set => metodoPagoNombre = value; }
    public decimal Costo { get => costo; set => costo = value; }
    public decimal Ganancia { get => ganancia; set => ganancia = value; }
    public decimal PorcentajeGanancia { get => porcentajeGanancia; set => porcentajeGanancia = value; }
}