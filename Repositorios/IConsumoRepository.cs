using ConsumoVM;

public interface IConsumoRepository
{
    void Crear(AltaConsumoVM vm);
    void Actualizar(ModificarConsumoVM vm);
    IEnumerable<ListarConsumoVM> ObtenerListado(DateTime fechaInicio, DateTime fechaFin);
    Ventas? ObtenerPorId(int id);
    void Eliminar(int id);
}
