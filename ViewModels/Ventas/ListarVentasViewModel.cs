using System.ComponentModel.DataAnnotations;
namespace PromocionesVM;

public class ListarVentasVM
{
    public long IdVenta { get; set; }
    [Display(Name = "Método de pago")]
    public string Metodo { get; set; } = string.Empty;
    [Display(Name = "Productos")]
    public string ProductosYPromociones { get; set; } = string.Empty;
    [DataType(DataType.Currency)]
    public decimal Precio { get; set; }
    [Display(Name = "Fecha")]
    [DisplayFormat(DataFormatString = "{0:dd/MM}")]
    public DateOnly Fecha { get; set; }
    [Display(Name = "Hora")]
    [DisplayFormat(DataFormatString = "{0:mm:HH}")]
    public TimeOnly Hora { get; set; }
    public ListarVentasVM() { }
}