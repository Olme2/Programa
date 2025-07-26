using VentasVM;

public interface IVentaRepository
{
    IEnumerable<ListarVentasVM> ObtenerListadoVentas(IndexVentasVM filtro);
    Ventas? ObtenerVentaPorId(long id);
    void CrearVenta(Ventas nuevaVenta);
    void ActualizarVenta(Ventas ventaModificada);
    void EliminarVenta(long id);   
}