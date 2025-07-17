namespace VentasVM;

public class AltaVentaVM
{
    private short idMetodoPago;
    private List<MetodosDePago> metodosPago;
    private string? detalle;
    private DateOnly fecha;
    private TimeOnly hora;
    public short IdMetodoPago { get => idMetodoPago; set => idMetodoPago = value; }
    public List<MetodosDePago> MetodosPago { get => metodosPago; set => metodosPago = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public TimeOnly Hora { get => hora; set => hora = value; }
}