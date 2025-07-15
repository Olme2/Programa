using entornoPolleria.Models;

namespace entornoPolleria.ViewModels
{
    public class EliminarProductosDePresupuestoViewModel
    {
        public int IdPresupuesto { get; set; }
        public int IdProducto { get; set; }
        public int CantidadVieja { get; set; }
        public int CantidadNueva { get; set; }
        public List<PresupuestosDetalle> Detalles { get; set; } = new List<PresupuestosDetalle>();
    }
} 