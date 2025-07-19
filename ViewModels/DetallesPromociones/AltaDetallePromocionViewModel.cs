using System.ComponentModel.DataAnnotations;
namespace DetallesPromocionesVM;

public class AltaDetallePromocionVM
    {
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
        public int IdProducto { get; set; }
        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(0.001, 999.999, ErrorMessage = "La cantidad debe ser como mínimo 0,001 y maximo 999,999.")]
        public int Cantidad { get; set; }
    }