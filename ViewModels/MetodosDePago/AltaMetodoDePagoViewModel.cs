using System.ComponentModel.DataAnnotations;
namespace MetodosVM;

public class AltaMetodoPagoVM
{
    [Required(ErrorMessage = "Nombre de Método de pago Obligatorio")]
    [StringLength(30, ErrorMessage = "El nombre del metodo de pago no puede ser mayor a 30 caracteres")]
    public string Metodo { get; set; }

    public AltaMetodoPagoVM()
    {
        Metodo = string.Empty;
    }

}