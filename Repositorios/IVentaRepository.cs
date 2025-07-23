using VentasVM;

public interface IVentaRepository
{
    IEnumerable<ListarVentasVM> ObtenerListadoVentas(IndexVentasVM filtro);
    Ventas? ObtenerVentaPorId(int id);
    void CrearVenta(Ventas nuevaVenta);
    void ActualizarVenta(Ventas ventaModificada);
    void EliminarVenta(int id);   
}