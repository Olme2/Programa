using ProduccionVM;

public interface IProduccionRepository
{
    void Crear(AltaProduccionVM vm);
    void Actualizar(ModificarProduccionVM vm);
    IEnumerable<ListarProduccionVM> ObtenerListado(DateTime fechaInicio, DateTime fechaFin);
    Ventas? ObtenerPorId(int id);
    void Eliminar(int id);
}
