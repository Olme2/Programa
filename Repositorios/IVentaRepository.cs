using VentasVM;

public interface IVentaRepository
{
    IEnumerable<ListarVentasVM> ObtenerListadoVentas(IndexVentasVM filtro);
    Ventas? ObtenerPorId(long id);
    void Crear(Ventas venta);
    void Actualizar(Ventas venta);
    void Eliminar(long id);   
}