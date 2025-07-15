public interface IProveedoresRepository
{
    List<Proveedores> ListarProveedores();
    void CrearNuevoProveedor(Proveedores proveedor);
    void ModificarProveedor(Proveedores proveedor);
    Proveedores ObtenerDetallesDeProveedorPorId(int id);
    void EliminarProveedorPorId(int id);
}