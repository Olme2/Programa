using System.ComponentModel.DataAnnotations;
namespace VentasPromocionesVM;

public class ModificarVentaPromocionVM
{
    [Required(ErrorMessage = "Debe seleccionar una promocion.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una promocion válida.")]
    public int IdPromocion { get; set; }
    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(0.001, 999.999, ErrorMessage = "La cantidad debe ser como mínimo 0,001 y maximo 999,999.")]
    public decimal Cantidad { get; set; }
    [Required(ErrorMessage = "El costo de la promo es obligatorio.")]
    [Range(0.01, 99999.99, ErrorMessage = "El costo de la promo debe ser como mínimo 0,01 y maximo 99999,99.")]
    public decimal CostoPromo { get; set; }
    [Required(ErrorMessage = "El precio de la promo es obligatorio.")]
    [Range(0.01, 99999.99, ErrorMessage = "El precio de la promo debe ser como mínimo 0,01 y maximo 99999,99.")]
    public decimal PrecioPromo { get; set; }
    
    public ModificarVentaPromocionVM() { }
    
    public ModificarVentaPromocionVM(VentasPromociones promocion)
    {
        IdPromocion = promocion.IdPromocion;
        Cantidad = promocion.Cantidad;
        CostoPromo = promocion.CostoPromo;
        PrecioPromo = promocion.PrecioPromo;
    }
}