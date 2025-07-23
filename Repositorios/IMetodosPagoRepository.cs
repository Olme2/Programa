using MetodosVM;
public interface IMetodosPagoRepository
{
    void Crear(MetodosPago metodoDePago);
    IEnumerable<ListarMetodosPagoVM> ObtenerListadoMetodosPago();
    MetodosPago? ObtenerPorId(short id);
    public void Actualizar(MetodosPago metodoDePago);
    void Eliminar(short id);
    bool PuedeSerEliminado(short id);
}
