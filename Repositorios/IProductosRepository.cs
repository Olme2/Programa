using ProductosVM;
public interface IProductosRepository
{
    IEnumerable<ListarProductosVM> ObtenerListadoProductos();
    Productos? ObtenerPorId(int id);
    void Crear(Productos producto);
    void Actualizar(Productos producto);
    void Eliminar(int id);
    bool PuedeSerEliminado(int id);
}