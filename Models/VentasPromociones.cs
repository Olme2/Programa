using VentasPromocionesVM;

public class VentasPromociones
{
    public long IdVenta { get; set; }
    public int IdPromocion { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoPromo { get; set; }
    public decimal PrecioPromo { get; set; }
    public virtual Promociones Promocion { get; set; }
    private VentasPromociones()
    {
        Promocion = null!;
    }

    private VentasPromociones(int idPromocion, decimal cantidad, decimal costoPromo, decimal precioPromo)
    {
        IdPromocion = idPromocion;
        Cantidad = cantidad;
        CostoPromo = costoPromo;
        PrecioPromo = precioPromo;
        Promocion = null!;
    }

    public static VentasPromociones CrearDesdeViewModel(VentaPromocionVM promocionVM)
    {
        return new VentasPromociones(promocionVM.IdPromocion, promocionVM.Cantidad, promocionVM.CostoPromo, promocionVM.PrecioPromo);
    }

    public decimal CalcularCosto()
    {
        return CostoPromo * Cantidad;
    }
    public decimal CalcularStock()
    {
        return Promocion.DetallesPromocion.Where(d => d.Cantidad > 0).Select(d => d.Producto.Stock / d.Cantidad).Min();
    }
    public decimal CalcularPrecio()
    {
        return PrecioPromo * Cantidad;
    }
}