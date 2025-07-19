using System.ComponentModel.DataAnnotations;
namespace ProveedoresVM;

public class AltaProveedorVM
{
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Proveedor { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El contacto no puede exceder los 100 caracteres.")]
    public string? Contacto { get; set; }
    [Range(0, 9999999.99, ErrorMessage = "El saldo no puede ser negativo.")]
    public decimal Debo { get; set; } = 0;
}