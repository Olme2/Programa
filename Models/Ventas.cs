using DetallesVentasVM;

public class Ventas
{
    private long idVenta;
    private short idMetodo;
    private decimal costo;
    private decimal precio;
    private DateOnly fecha;
    private TimeOnly hora;
    private string? detalle;
    private List<DetallesVentas> detallesVenta;
    private List<VentasPromociones> ventasPromociones;

    public Ventas()
    {
        detallesVenta = new List<DetallesVentas>();
        ventasPromociones = new List<VentasPromociones>();
    }

    public long IdVenta { get => idVenta; set => idVenta = value; }
    public short IdMetodo { get => idMetodo; set => idMetodo = value; }
    public decimal Costo { get => costo; set => costo = value; }
    public decimal Precio { get => precio; set => precio = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public TimeOnly Hora { get => hora; set => hora = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
    public List<DetallesVentas> DetallesVenta { get => detallesVenta; set => detallesVenta = value; } 
    public List<VentasPromociones> VentasPromociones { get => ventasPromociones; set => ventasPromociones = value; }
}

