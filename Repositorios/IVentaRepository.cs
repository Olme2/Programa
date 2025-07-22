using PromocionesVM;

namespace entornoPolleria.Repositorios
{
    public interface IVentaRepository
    {
        IEnumerable<ListarVentasVM> ObtenerVentasPorFechas(DateOnly inicio, DateOnly fin);
        Ventas? ObtenerVentaPorId(int id);
        void CrearVenta(Ventas nuevaVenta);
        void ActualizarVenta(Ventas ventaModificada);
        void EliminarVenta(int id);
    }
}