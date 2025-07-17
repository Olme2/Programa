public interface IProductosRepository
{
    void CrearNuevoProducto(Productos producto);
    void ModificarProducto(Productos producto);
    List<Productos> ListarProductosRegistrados();
    Productos ObtenerDetallesDeProductoPorId(int id);
    void EliminarProductoPorId(int id);
}