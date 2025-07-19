public interface IProveedoresRepository
{
    IEnumerable<Proveedores> ObtenerTodos();
    Proveedores? ObtenerPorId(int id);
    void Crear(Proveedores proveedor);
    void Actualizar(Proveedores proveedor);
    void Eliminar(int id);
}