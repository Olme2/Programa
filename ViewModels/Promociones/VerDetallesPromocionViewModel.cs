using System.ComponentModel.DataAnnotations;
using DetallesPromocionesVM; // Asegúrate de tener este using

namespace PromocionesVM
{
    /// <summary>
    /// ViewModel específico para la vista "Ver Detalles" de una promoción.
    /// </summary>
    public class VerDetallesPromocionVM
    {
        public int IdPromocion { get; set; }
        public string Promocion { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [DataType(DataType.Currency)]
        public decimal CostoTotal { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateOnly Inicio { get; set; }

        [Display(Name = "Fecha de Fin")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", NullDisplayText = "Indefinido")]
        public DateOnly? Fin { get; set; }
        
        public bool EsEliminable { get; set; }

        // Contendrá la lista de todos los productos de la promoción
        public List<ListarDetallesPromocionVM> Detalles { get; set; }

        public VerDetallesPromocionVM()
        {
            Detalles = new List<ListarDetallesPromocionVM>();
        }

        // Constructor que mapea desde el modelo de dominio
        public VerDetallesPromocionVM(Promociones promocion, bool esEliminable)
        {
            IdPromocion = promocion.IdPromocion;
            Promocion = promocion.Promocion;
            Precio = promocion.Precio;
            Inicio = promocion.Inicio;
            Fin = promocion.Fin;
            EsEliminable = esEliminable;
            
            // Usamos los métodos del dominio y de los ViewModels para los cálculos
            CostoTotal = promocion.CalcularCostoTotal();
            Detalles = promocion.DetallesPromocion
                                .Select(d => new ListarDetallesPromocionVM(d))
                                .ToList();
        }
    }
}