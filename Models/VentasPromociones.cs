using VentasPromocionesVM;

public class VentasPromociones
{
    public long IdVenta { get; set; }
    public int IdPromocion { get; set; }
    public short Cantidad { get; set; }
    public decimal CostoPromo { get; set; }
    public decimal PrecioPromo { get; set; }
    public virtual Promociones Promocion { get; set; }
    private VentasPromociones()
    {
        Promocion = null!;
    }

    private VentasPromociones(int idPromocion, short cantidad, decimal costoPromo, decimal precioPromo)
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

    public static VentasPromociones CrearDesdeViewModel(ModificarVentaPromocionVM promocionVM)
    {
        return new VentasPromociones(promocionVM.IdPromocion, promocionVM.Cantidad, promocionVM.CostoPromo, promocionVM.PrecioPromo);
    }

    public decimal CalcularCosto()
    {
        return CostoPromo * Cantidad;
    }

    public decimal CalcularPrecio()
    {
        return PrecioPromo * Cantidad;
    }
}