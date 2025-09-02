using ComprasVM;
public interface IComprasRepository
{
    IEnumerable<ListarComprasVM> ObtenerListadoCompras(IndexComprasVM filtro);
    Compras? ObtenerPorId(int id);
    void Crear(Compras compra);
    void Actualizar(Compras compra);
    void Eliminar(int id);
}