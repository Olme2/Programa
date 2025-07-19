using System.ComponentModel.DataAnnotations;

namespace DetallesPromocionesVM;
public class ModificarDetallePromocionVM
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
    public int IdProducto { get; set; }
    [Required]
    [Range(0.001, 999.999, ErrorMessage = "La cantidad debe ser al menos 0,001 y como maximo 999,999.")]
    public decimal Cantidad { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
}