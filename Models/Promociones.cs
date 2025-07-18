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

    public Promociones(string Promocion, decimal Precio, DateOnly Inicio, DateOnly? Fin, List<DetallesPromociones> Detalles)
    {
        promocion = Promocion;
        precio = Precio;
        inicio = Inicio;
        fin = Fin;
        detallesPromocion = Detalles;
    }
    public static Promociones CrearDesdeViewModel(AltaPromocionVM promocionVM)
    {
        var detalles = promocionVM.DetallesPromocion.Select(detalleVM => new DetallesPromociones(detalleVM)).ToList();
        return new Promociones(promocionVM.Promocion,promocionVM.Precio,promocionVM.Inicio,promocionVM.Fin,detalles);
    }

    public Promociones(ModificarPromocionVM promocionVM)
    {
        idPromocion = promocionVM.IdPromocion;
        promocion = promocionVM.Promocion;
        precio = promocionVM.Precio;
        inicio = promocionVM.Inicio;
        fin = promocionVM.Fin;
        detallesPromocion = promocionVM.DetallesPromocion.Select(d => new DetallesPromociones(d)).ToList();
    }

    public decimal CalcularCostoTotal()
    {
        return detallesPromocion.Sum(detalle => detalle.CalcularCosto());
    }

    public int IdPromocion { get => idPromocion; set => idPromocion = value; }
    public string Promocion { get => promocion; set => promocion = value; }
    public decimal Precio { get => precio; set => precio = value; }
    public DateOnly Inicio { get => inicio; set => inicio = value; }
    public DateOnly? Fin { get => fin; set => fin = value; }
    public List<DetallesPromociones> DetallesPromocion { get => detallesPromocion; set => detallesPromocion = value; }
}