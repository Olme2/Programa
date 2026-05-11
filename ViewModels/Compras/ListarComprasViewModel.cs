using System.ComponentModel.DataAnnotations;
namespace ComprasVM;

public class ListarComprasVM
{
    public int IdCompra { get; set; }

    [Display(Name = "Productos")]
    public string Productos { get; set; } = string.Empty;

    [Display(Name = "Proveedor")]
    public string Proveedor { get; set; } = string.Empty;

    [DataType(DataType.Currency)]
    public decimal Total { get; set; }

    [Display(Name = "Fecha")]
    [DisplayFormat(DataFormatString = "{0:dd/MM}")]
    public DateOnly Fecha { get; set; }

    public string? Detalle { get; set; }
    public bool Pagada { get; set; }

    public ListarComprasVM() { }

    public ListarComprasVM(Compras compra)
    {
        IdCompra = compra.IdCompra;
        Productos = string.Concat("Productos: ", string.Join(",", compra.DetallesCompra.Select(dc => dc.Producto.Producto)));
        Proveedor = compra.Proveedor.Proveedor;
        Total = compra.CalcularTotal();
        Fecha = compra.Fecha;
        Detalle = compra.Detalle;
        Pagada = compra.Pagada;
    }
}
