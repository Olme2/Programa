using System.ComponentModel.DataAnnotations;
namespace MetodosVM;

public class ModificarMetodoPago
{
    [Required]
    public short IdMetodo { get; set; }
    [Required(ErrorMessage = "Nombre de Método de pago Obligatorio")]
    [StringLength(30, ErrorMessage = "El nombre del metodo de pago no puede ser mayor a 30 caracteres")]
    public string Metodo { get; set; }

    public ModificarMetodoPago()
    {
        Metodo = string.Empty;
    }
    public ModificarMetodoPago(MetodosPago metodoPago)
    {
        IdMetodo = metodoPago.IdMetodo;
        Metodo = metodoPago.Metodo;
    }

    
}