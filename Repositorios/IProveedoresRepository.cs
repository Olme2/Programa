using ProveedoresVM;
public interface IProveedoresRepository
{
    IEnumerable<ListarProveedoresVM> ObtenerListadoProveedores();
    Proveedores? ObtenerPorId(int id);
    void Crear(Proveedores proveedor);
    void Actualizar(Proveedores proveedor);
    void Eliminar(int id);
}