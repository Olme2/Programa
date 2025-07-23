using System.ComponentModel.DataAnnotations;
namespace DetallesVentasVM;

public class ModificarDetalleVentaVM
{
    [Required(ErrorMessage = "Debe seleccionar un producto.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
    public int IdProducto { get;  set; }
    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(0.001, 999.999, ErrorMessage = "La cantidad debe ser como mínimo 0,001 y maximo 999,999.")]
    public string? NombreProducto { get; set; }
    public decimal Cantidad { get;  set; }
    [Required(ErrorMessage = "El costo unitario es obligatorio.")]
    [Range(0.01, 99999.99, ErrorMessage = "El costo unitario debe ser como mínimo 0,01 y maximo 99999,99.")]
    public decimal CostoUnitario { get;  set; }
    [Required(ErrorMessage = "El precio unitario es obligatorio.")]
    [Range(0.01, 99999.99, ErrorMessage = "El precio unitario debe ser como mínimo 0,01 y maximo 99999,99.")]
    public decimal PrecioUnitario { get;  set; }

    public ModificarDetalleVentaVM() { }
    
    public ModificarDetalleVentaVM(DetallesVentas detalle)
    {
        IdProducto = detalle.IdProducto;
        Cantidad = detalle.Cantidad;
        if (detalle.Producto != null)
        {
            NombreProducto = detalle.Producto.Producto;
            CostoUnitario = detalle.Producto.Costo;
            PrecioUnitario = detalle.Producto.Precio;
        }
    }
}