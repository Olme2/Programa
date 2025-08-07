using System.ComponentModel.DataAnnotations;
namespace VentasPromocionesVM;

public class VentaPromocionVM
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una promoción.")]
    public int IdPromocion { get; set; }

    [Required]
    [Range(0.001, 999.999, ErrorMessage = "La cantidad debe ser como mínimo 0,001 y maximo 999,999.")]
    public decimal Cantidad { get; set; }
    [Required]
    [Range(0.01, 99999.99, ErrorMessage = "El precio debe ser entre 0.01 y 99.999,99.")]
    public decimal CostoPromo { get; set; }
    [Required]
    [Range(0.01, 99999.99, ErrorMessage = "El precio debe ser entre 0.01 y 99.999,99.")]
    public decimal PrecioPromo { get; set; }
    public string NombrePromocion { get; set; } = string.Empty;
    public VentaPromocionVM() { }
    public VentaPromocionVM(VentasPromociones p)
    {
        IdPromocion = p.IdPromocion;
        NombrePromocion = $"{p.Promocion} (${p.PrecioPromo.ToString("N2", CG.CulturaES)}) - S: {p.CalcularStock().ToString("N2", CG.CulturaES)}";
        Cantidad = p.Cantidad;
        PrecioPromo = p.PrecioPromo;
        CostoPromo = p.CostoPromo;
    }
}