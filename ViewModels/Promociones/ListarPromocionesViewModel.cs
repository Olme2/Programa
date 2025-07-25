using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Html;
namespace PromocionesVM;

public class ListarPromocionesVM
{
    public int IdPromocion { get; set; }
    public string Promocion { get; set; } = string.Empty;
    [Display(Name = "Productos")]
    public HtmlString ProductosConcatenados { get; set; } = new HtmlString("");
    [DataType(DataType.Currency)]
    public decimal Costo { get; set; }
    [DataType(DataType.Currency)]
    public decimal Precio { get; set; }
    [DataType(DataType.Currency)]
    public decimal Ganancia { get; set; }
    [Display(Name = "% Ganancia")]
    [DisplayFormat(DataFormatString = "{0:P2}")]
    public decimal PorcentajeGanancia { get; set; }
    [Display(Name = "Fecha de Inicio")]
    [DisplayFormat(DataFormatString = "{0:dd/MM}")]
    public DateOnly Inicio { get; set; }
    [Display(Name = "Fecha de Fin")]
    [DisplayFormat(DataFormatString = "{0:dd/MM}", NullDisplayText = "-")]
    public DateOnly? Fin { get; set; }
    public bool Activa { get; set; }
    public bool EsEliminable { get; set; }
    public int Stock { get; set; }
    public int VentaSemanal {get; set; }
    public ListarPromocionesVM() { }
}