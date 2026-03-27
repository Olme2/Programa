using ConsumoVM;

public interface IConsumoRepository
{
    void Crear(AltaConsumoVM vm);
    IEnumerable<ListarConsumoVM> ObtenerListado(DateTime fechaInicio, DateTime fechaFin);
    Ventas? ObtenerPorId(int id);
    void Eliminar(int id);
}
