using Microsoft.AspNetCore.Mvc.Rendering;
namespace VentasVM;

public class ModificarVentaVM
{
    private long idVenta;
    private short idMetodoPago;
    private List<SelectListItem> metodosPago;
    private string detalle;
    private DateOnly fecha;
    private TimeOnly hora;
    public long IdVenta { get => idVenta; set => idVenta = value; }
    public short IdMetodoPago { get => idMetodoPago; set => idMetodoPago = value; }
    public List<SelectListItem> MetodosPago { get => metodosPago; set => metodosPago = value; }
    public string Detalle { get => detalle; set => detalle = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public TimeOnly Hora { get => hora; set => hora = value; }
}