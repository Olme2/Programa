using System.ComponentModel.DataAnnotations;
using DetallesPromocionesVM; 
using ProductosVM;

namespace PromocionesVM;
public class ModificarPromocionVM
{
    [Required]
    public int IdPromocion { get; set; }
    [Required(ErrorMessage = "El nombre de la promoción es obligatorio.")]
    [StringLength(50)]
    public string Promocion { get; set; }
    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 99999.99)]
    public decimal Precio { get; set; }
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly Inicio { get; set; }
    public DateOnly? Fin { get; set; }
    [MinLength(1, ErrorMessage = "La promoción debe tener al menos un producto.")]
    public List<ModificarDetallePromocionVM> DetallesPromocion { get; set; }
    // Propiedad para la lista de TODOS los productos disponibles.
    public List<ListarProductosVM> Productos { get; set; }
    public ModificarPromocionVM()
    {
        Promocion = string.Empty;
        DetallesPromocion = new List<ModificarDetallePromocionVM>();
        Productos = new List<ListarProductosVM>();
    }
    // Constructor que mapea la entidad al ViewModel.
    public ModificarPromocionVM(Promociones promocion) : this()
    {
        IdPromocion = promocion.IdPromocion;
        Promocion = promocion.Promocion;
        Precio = promocion.Precio;
        Inicio = promocion.Inicio;
        Fin = promocion.Fin;
        DetallesPromocion = promocion.DetallesPromocion.Select(d => new ModificarDetallePromocionVM(d)).ToList();
    }
}