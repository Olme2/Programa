public interface IPresupuestosRepository{
    void CrearPresupuesto(Presupuestos presupuesto);
    Presupuestos ObtenerPresupuestoPorId(int id);
    void AgregarProducto(int idPresupuesto, int idProducto, int cantidad);
    void EliminarPresupuestoPorId(int id);
    List<PresupuestosDetalle> MostrarDetallePorId(int id);
    void EliminarProducto(int idPresupuesto, int idProducto, int cantVieja, int cantNueva);
    void ModificarPresupuesto(Presupuestos presupuesto);
}