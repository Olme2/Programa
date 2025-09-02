using ProductosVM;
public interface IProductosRepository
{
    IEnumerable<ListarProductosVM> ObtenerListadoProductos();
    IEnumerable<ListarProductosVM> ObtenerListadoProductos(IndexProductosVM filtro);
    Productos? ObtenerPorId(int id);
    void Crear(Productos producto);
    void Actualizar(Productos producto);
    void Eliminar(int id);
    bool PuedeSerEliminado(int id);
    IEnumerable<(string Nombre, decimal Precio)> ObtenerProductosActivosParaLista();
}