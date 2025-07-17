public interface IPromocionesRepository
{
    void CrearNuevaPromocion(Promociones promocion);
    void ModificarPromocion(Promociones promocion);
    List<Promociones> ListarPromocionesRegistradas();
    Promociones ObtenerDetallesDePromocionPorId(int id);
    void EliminarPromocionPorId(int id);
} 