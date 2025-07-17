using DetallesComprasVM;
using ProductosVM;

namespace ComprasVM;

public class AltaCompraVM
{
    private int idProveedor;
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
    
    public int IdProveedor { get => idProveedor;  set => idProveedor = value; }
    public decimal Total { get => total; set => total = value; }
    public DateOnly Fecha { get => fecha; set => fecha = value; }
    public string? Detalle { get => detalle; set => detalle = value; }
    public List<ListarDetallesCompraVM> Detalles { get => detalles; set => detalles = value; }
}