namespace VentasVM;

public class ListarVentasVM
{
    private long idVenta;
    private decimal costo;
    private decimal precio;
    private decimal ganancia;
    private decimal porcentajeGanancia;
    private DateOnly fecha;
    private TimeOnly hora;
    private string? detalle;
    private string metodoDePago;
    private List<string> productosVendidos;

    public ListarVentasVM()
    {
        productosVendidos = new List<string>();
        metodoDePago = string.Empty;
    }
    public ListarVentasVM(Ventas venta, string MetodoDePago, List<string> ProductosVentidos)
    {
        idVenta = venta.IdVenta;
        precio = venta.Precio;
        fecha = venta.Fecha;
        hora = venta.Hora;
        metodoDePago = MetodoDePago;
    }

    public long IdVenta { get => idVenta; set => idVenta = value; }
    public decimal Precio { get => precio; set => precio = value; }
    public decimal Costo { get => costo; set => costo = value; }
    public decimal Ganancia { get => ganancia; set => ganancia = value; }
    public decimal PorcentajeGanancia { get => porcentajeGanancia; set => porcentajeGanancia = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public TimeOnly Hora { get => hora; set => hora = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
    public string MetodoDePago { get => metodoDePago; set => metodoDePago = value; }
    public List<string> ProductosVendidos { get => productosVendidos; set => productosVendidos = value; }
}