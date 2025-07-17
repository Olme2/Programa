namespace VentasVM;

public class AltaVentaVM
{
    private short idMetodoPago;
    private DateOnly fecha;
    private TimeOnly hora;
    private string? detalle;
    private List<DetallesVentas> detallesVentas;
    private List<MetodosDePago> metodosDePago;

    public AltaVentaVM()
    {
        detallesVentas = new List<DetallesVentas>();
        metodosDePago = new List<MetodosDePago>();
    }
    public AltaVentaVM(List<MetodosDePago> MetodosDePago)
    {
        fecha = DateOnly.FromDateTime(DateTime.Today);
        hora = TimeOnly.FromDateTime(DateTime.Now);
        detallesVentas = new List<DetallesVentas>();
        metodosDePago = MetodosDePago;
    }
    public short IdMetodoPago { get => idMetodoPago; set => idMetodoPago = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public TimeOnly Hora { get => hora; set => hora = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
    public List<DetallesVentas> DetallesVentas { get => detallesVentas; set => detallesVentas = value; }
    public List<MetodosDePago> MetodosDePago { get => metodosDePago; set => metodosDePago = value;}

}