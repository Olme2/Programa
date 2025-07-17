public interface IVentaRepository
{
    List<Ventas> ListarVentasRegistradas();
    Ventas ObtenerDetallesDeVentaPorId(long id);
    void CrearNuevaVenta(Ventas venta);
    void ModificarVenta(Ventas venta);
    void EliminarVentaPorId(long id);
    List<Ventas> ListarVentasEntreDosFechas(DateOnly fechaInicio, DateOnly fechaFin);
}