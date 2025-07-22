using PromocionesVM;

public class Promociones
{
    public int IdPromocion { get; private set; }
    public string Promocion { get; private set; }
    public decimal Precio { get; private set; }
    public DateOnly Inicio { get; private set; }
    public DateOnly? Fin { get; private set; }
    private List<DetallesPromociones> _detallesPromocion = new List<DetallesPromociones>();
    public IReadOnlyCollection<DetallesPromociones> DetallesPromocion => _detallesPromocion.AsReadOnly();

    private Promociones()
    {
        Promocion = string.Empty;
    }

    private Promociones(string nombrePromocion, decimal precio, DateOnly inicio, DateOnly? fin, List<DetallesPromociones> detalles)
    {
        Promocion = nombrePromocion;
        Precio = precio;
        Inicio = inicio;
        Fin = fin;
        _detallesPromocion = detalles;
    }

    public static Promociones CrearDesdeViewModel(AltaPromocionVM promocionVM)
    {
        var detalles = promocionVM.DetallesPromocion.Select(DetallesPromociones.CrearDesdeViewModel).ToList();
        return new Promociones(promocionVM.Promocion, promocionVM.Precio, promocionVM.Inicio, promocionVM.Fin, detalles);
    }

    public void ActualizarDesdeViewModel(ModificarPromocionVM promocionVM)
    {
        Promocion = promocionVM.Promocion;
        Precio = promocionVM.Precio;
        Inicio = promocionVM.Inicio;
        Fin = promocionVM.Fin;
        var detallesActualizados = promocionVM.DetallesPromocion.Select(DetallesPromociones.CrearDesdeViewModel).ToList();
        LimpiarDetalles();
        foreach (var detalle in detallesActualizados)
        {
            AgregarDetalle(detalle);
        }
    }

    public void LimpiarDetalles()
    {
        _detallesPromocion.Clear();
    }

    public void AgregarDetalle(DetallesPromociones detalle)
    {
        _detallesPromocion.Add(detalle);
    }

    public decimal CalcularCostoTotal()
    {
        return _detallesPromocion.Sum(detalle => detalle.CalcularCosto());
    }

    public void Desactivar()
    {
        Fin = DateOnly.FromDateTime(DateTime.Now);
    }
}