using PromocionesVM;
public interface IPromocionesRepository
{
    IEnumerable<ListarPromocionesVM> ObtenerListadoPromociones();
    Promociones? ObtenerPorId(int id);
    void Crear(Promociones promocion);
    void Actualizar(Promociones promocion);
    void Eliminar(int id);
    bool PuedeSerEliminada(int id);
    void DesactivarPorIdProducto(int id);
}