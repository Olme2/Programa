using DetallesPromocionesVM;
using ProductosVM;

namespace PromocionesVM;

public class ModificarPromocionVM
{
    private int idPromocion;
    private string promocion;
    private decimal costo;
    private decimal precio;
    private decimal ganancia;
    private decimal porcentajeGanancia;
    private DateOnly inicio;
    private DateOnly? fin;
    private List<ModificarDetallePromocionVM> detallesPromocion;
    private List<ListarProductosVM> productos;

    public ModificarPromocionVM()
    {
        promocion = string.Empty;
        detallesPromocion = new List<ModificarDetallePromocionVM>();
        productos = new List<ListarProductosVM>();
    }

    public ModificarPromocionVM(Promociones Promocion, decimal Costo, List<ModificarDetallePromocionVM> DetallesPromocion, List<ListarProductosVM> Productos)
    {
        idPromocion = Promocion.IdPromocion;
        promocion = Promocion.Promocion;
        costo = Costo;
        precio = Promocion.Precio;
        ganancia = precio - Costo;
        porcentajeGanancia = (ganancia - Costo / Costo) * 100;
        inicio = Promocion.Inicio;
        fin = Promocion.Fin;
        detallesPromocion = DetallesPromocion;
        productos = Productos;
    }
    
    public int IdPromocion { get => idPromocion; set => idPromocion = value; }
    public string Promocion { get => promocion; set => promocion = value; }
    public decimal Costo { get => costo; set => costo = value; }
    public decimal Precio { get => precio; set => precio = value; }
    public decimal Ganancia { get => ganancia; set => ganancia = value; }
    public decimal PorcentajeGanancia { get => porcentajeGanancia; set => porcentajeGanancia = value; }
    public DateOnly Inicio { get => inicio; set => inicio = value; }
    public DateOnly? Fin { get => fin; set => fin = value; }
    public List<ModificarDetallePromocionVM> DetallesPromocion { get => detallesPromocion; set => detallesPromocion = value; }
    public List<ListarProductosVM> Productos { get => productos; set => productos = value; }
}