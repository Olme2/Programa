public interface IVentasRepository
{
    void CrearVenta(Ventas venta, List<DetallesVentas> detalles);
    void ModificarVenta(Ventas venta, List<DetallesVentas> nuevosDetalles);
    void EliminarVenta(long idVenta);
    Ventas? ObtenerVentaPorId(long idVenta);
    List<Ventas> ListarVentas();
    List<DetallesVentas> ObtenerDetallesPorVenta(long idVenta);
    bool ValidarStock(List<DetallesVentas> detalles, out string mensajeError);
    bool ValidarMetodoDePago(short idMetodo);
} 