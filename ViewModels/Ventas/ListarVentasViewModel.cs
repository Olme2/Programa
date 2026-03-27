using Microsoft.AspNetCore.Html;
using System.ComponentModel.DataAnnotations;
namespace VentasVM;

public class ListarVentasVM
{
    public long IdVenta { get; set; }

    [Display(Name = "Método de pago")]
    public string Metodo { get; set; } = string.Empty;

    [Display(Name = "Promociones/Productos")]
    public string ProductosYPromociones { get; set; } = string.Empty;

    [DataType(DataType.Currency)]
    public decimal Total { get; set; }

    [DataType(DataType.Currency)]
    public decimal Costo { get; set; }

    [Display(Name = "Fecha")]
    [DisplayFormat(DataFormatString = "{0:dd/MM}")]
    public DateOnly Fecha { get; set; }

    [Display(Name = "Hora")]
    [DisplayFormat(DataFormatString = "{0:HH:mm}")]
    public TimeOnly Hora { get; set; }

    public short IdMetodo { get; set; }

    public string? Detalle { get; set; }

    public decimal Redondeo { get; set; }
    
    public ListarVentasVM() { }
    public ListarVentasVM(Ventas venta)
    {
        IdVenta = venta.IdVenta;
        Metodo = venta.Metodo.Metodo;
        ProductosYPromociones = string.Concat("Promociones: ", string.Join(", ", venta.VentaPromociones.Select(vp => vp.Promocion.Promocion)), "<br>Productos: ", string.Join(",", venta.DetallesVenta.Select(dv => dv.Producto.Producto)));
        Total = venta.CalcularPrecioTotal();
        Fecha = venta.Fecha;
        Hora = venta.Hora;
        Redondeo = venta.Redondeo;
    }
}