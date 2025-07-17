using PromocionesVM;

public class Promociones
{
    private int idPromocion;
    private string promocion;
    private decimal precio;
    private DateOnly inicio;
    private DateOnly? fin;
    private List<DetallesPromociones> detallesPromocion;

    public Promociones()
    {
        promocion = string.Empty;
        inicio = DateOnly.FromDateTime(DateTime.Now);
        detallesPromocion = new List<DetallesPromociones>();
    }

    public Promociones(AltaPromocionVM promocionVM)
    {
        promocion = promocionVM.Promocion;
        precio = promocionVM.Precio;
        inicio = promocionVM.Inicio;
        detallesPromocion = promocionVM.DetallesPromocion;
    }
    public int IdPromocion { get => idPromocion; set => idPromocion = value; }
    public string Promocion { get => promocion; set => promocion = value; }
    public decimal Precio { get => precio; set => precio = value; }
    public DateOnly Inicio { get => inicio; set => inicio = value; }
    public DateOnly? Fin { get => fin; set => fin = value; }
    public List<DetallesPromociones> DetallesPromocion { get => detallesPromocion; set => detallesPromocion = value; }
}