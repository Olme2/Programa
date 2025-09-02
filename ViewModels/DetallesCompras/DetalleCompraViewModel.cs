using System.ComponentModel.DataAnnotations;
namespace DetallesComprasVM;

public class DetalleCompraVM
{
    [Required(ErrorMessage = "Debe seleccionar un producto.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
    public int IdProducto { get; set; }
    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(0.001, 9999.999, ErrorMessage = "La cantidad debe ser entre 0,001 y 9.999,999.")]
    public decimal Cantidad { get; set; }
    [Required(ErrorMessage = "El precio de costo es obligatorio.")]
    [DataType(DataType.Currency)]
    [Range(0.01, 999999.99, ErrorMessage = "El precio de costo debe ser como mínimo $0,01 y máximo $999.999,99.")]
    public decimal CostoUnitario { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public DetalleCompraVM() { }

    public DetalleCompraVM(DetallesCompras detalle)
    {
        IdProducto = detalle.IdProducto;
        Cantidad = detalle.Cantidad;
        CostoUnitario = detalle.CostoUnitario;
        NombreProducto = detalle.Producto.Producto ?? "N/A";
    }
}