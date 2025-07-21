using System.ComponentModel.DataAnnotations;

namespace DetallesPromocionesVM;

public class ModificarDetallePromocionVM
{
    [Required]
    [Range(1, int.MaxValue)]
    public int IdProducto { get; set; }

    [Required]
    [Range(0.001, 99999.999)]
    public decimal Cantidad { get; set; }

    public string? NombreProducto { get; set; }
    
    // Propiedad clave para que la vista conozca el costo inicial del producto.
    public decimal Costo { get; set; }

    public ModificarDetallePromocionVM() { }

    // Constructor que facilita el mapeo desde la entidad en el controlador.
    public ModificarDetallePromocionVM(DetallesPromociones detalle)
    {
        IdProducto = detalle.IdProducto;
        Cantidad = detalle.Cantidad;
        if (detalle.Producto != null)
        {
            NombreProducto = detalle.Producto.Producto;
            Costo = detalle.Producto.Costo;
        }
    }
}