using DetallesComprasVM;
using ProductosVM;

namespace ComprasVM;

public class AltaCompraVM
{
    private decimal total;
    private DateOnly fecha;
    private string? detalle;
    private List<ListarDetallesCompraVM> detalles;

    public AltaCompraVM()
    {
        total = 0;
        fecha = DateOnly.FromDateTime(DateTime.Now);
        detalles = new List<ListarDetallesCompraVM>();
    }
    
    public decimal Total { get => total; set => total = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
    public List<ListarDetallesCompraVM> Detalles { get => detalles; set => detalles = value; }
}