namespace PromocionesVM;
using System.ComponentModel.DataAnnotations;
using ProductosVM;

public class AltaPromocionVM
{
    private string promocion;
    private decimal precio;
    private DateOnly inicio;
    private List<DetallesPromociones> detallesPromocion;
    private List<ListarProductosVM> productos;

    public AltaPromocionVM()
    {
        promocion = string.Empty;
        inicio = DateOnly.FromDateTime(DateTime.Now);
        detallesPromocion = new List<DetallesPromociones>();
        productos = new List<ListarProductosVM>();
    }

    public AltaPromocionVM(List<ListarProductosVM> Productos)
    {
        promocion = string.Empty;
        inicio = DateOnly.FromDateTime(DateTime.Now);
        detallesPromocion = new List<DetallesPromociones>();
        productos = Productos;
    }

    [Required(ErrorMessage = "Nombre de promoción obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre de la promoción no puede ser más largo que 50 caracteres.")]
    public string Promocion { get => promocion; set => promocion = value; }
    [Required(ErrorMessage = "Precio de la promoción obligatorio.")]
    [Range(0.01, 99999.99, ErrorMessage = "El precio debe estar entre 0,01 y 99.999,99.")]
    public decimal Precio { get => precio; set => precio = value; }
    [Required(ErrorMessage = "Fecha de inicio de la promoción obligatoria.")]
    public DateOnly Inicio { get => inicio; set => inicio = value; }
    [MinLength(1, ErrorMessage = "La promoción debe tener al menos un producto asociado.")]
    public List<DetallesPromociones> DetallesPromocion { get => detallesPromocion; set => detallesPromocion = value; }
    public List<ListarProductosVM> Productos { get => productos; set => productos = value; }
}