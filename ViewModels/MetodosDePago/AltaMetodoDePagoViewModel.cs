using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MetodosVM;

public class AltaMetodoDePagoVM
{
    private string metodo;

    public AltaMetodoDePagoVM()
    {
        metodo = string.Empty;
    }

    [Required(ErrorMessage = "Nombre de Método de pago Obligatorio")]
    [StringLength(30, ErrorMessage = "El nombre del metodo de pago no puede ser mayor a 30 caracteres")]
    public string Metodo { get => metodo; set => metodo = value; }
}