public interface IMetodosPagoRepository
{
    void CrearMetodoDePago(MetodosDePago metodoDePago);
    List<MetodosDePago> ListarMetodosDePagoRegistrados();
    MetodosDePago ObtenerDetallesDeMetodoDePagoPorId(short id);
    public void ModificarMetodoDePago(MetodosDePago metodoDePago);
    void EliminarMetodoDePagoPorId(short id);
}
