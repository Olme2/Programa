public interface IDetalleVentaRepository
{
    List<DetallesVentas> GetByVentaId(long idVenta);
    DetallesVentas GetById(long idVenta, int idProducto);
    void Add(DetallesVentas detalle);
    void Update(DetallesVentas detalle);
    void Delete(long idVenta, int idProducto);
}