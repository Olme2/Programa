using ProduccionVM;

public interface IProduccionRepository
{
    void Crear(AltaProduccionVM vm);
    IEnumerable<ListarProduccionVM> ObtenerListado(DateTime fechaInicio, DateTime fechaFin);
    Ventas? ObtenerPorId(int id);
    void Eliminar(int id);
}
