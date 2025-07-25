using System.ComponentModel.DataAnnotations;
namespace VentasPromocionesVM;

public class VentaPromocionVM
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una promoción.")]
    public int IdPromocion { get; set; }

    [Required]
    [Range(1, short.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public short Cantidad { get; set; }
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
        NombrePromocion = $"{p.Promocion} (${p.PrecioPromo.ToString("N2", ConfiguracionGlobal.CulturaES)}) - S: {p.CalcularStock().ToString("N2", ConfiguracionGlobal.CulturaES)}";
        Cantidad = p.Cantidad;
        PrecioPromo = p.PrecioPromo;
        CostoPromo = p.CostoPromo;
    }
}