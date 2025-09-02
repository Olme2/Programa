using MetodosVM;
public interface IMetodosPagoRepository
{
    IEnumerable<ListarMetodosPagoVM> ObtenerListadoMetodosPago();
    MetodosPago? ObtenerPorId(short id);
    void Crear(MetodosPago metodoDePago);
    public void Actualizar(MetodosPago metodoDePago);
    void Eliminar(short id);
    bool PuedeSerEliminado(short id);
}
