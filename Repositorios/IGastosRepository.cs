using GastosVM;

public interface IGastosRepository
{
    void Crear(AltaGastoVM vm);
    IEnumerable<ListarGastoVM> ObtenerListado(DateTime fechaInicio, DateTime fechaFin);
    void Eliminar(int id);
}
