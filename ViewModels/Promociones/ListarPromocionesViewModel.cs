namespace PromocionesVM;

public class ListarPromocionesVM
{
    private int idPromocion;
    private string promocion;
    private decimal costo;
    private decimal precio;
    private DateOnly inicio;
    private DateOnly? fin;
    private List<string> productos;

    public ListarPromocionesVM()
    {
        promocion = string.Empty;
        productos = new List<string>();
    }

    public ListarPromocionesVM(Promociones Promocion, decimal Costo, List<string> Productos)
    {
        idPromocion = Promocion.IdPromocion;
        promocion = Promocion.Promocion;
        costo = Costo;
        precio = Promocion.Precio;
        inicio = Promocion.Inicio;
        fin = Promocion.Fin;
        productos = Productos;
    }

    public int IdPromocion { get => idPromocion; set => idPromocion = value; }
    public string Promocion { get => promocion; set => promocion = value; }
    public decimal Costo { get => costo; set => costo = value; }
    public decimal Precio { get => precio; set => precio = value; }
    public DateOnly Inicio { get => inicio;  set => inicio = value; }
    public DateOnly? Fin { get => fin; set => fin = value; }
    public List<string> Productos { get => productos; set => productos = value; }
}