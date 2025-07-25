using System.ComponentModel.DataAnnotations;

namespace DetallesPromocionesVM;

public class DetallePromocionVM
{
    [Required(ErrorMessage = "Debe seleccionar un producto.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un producto válido.")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(0.001, 999.999, ErrorMessage = "La cantidad debe ser como mínimo 0,001 y maximo 999,999.")]
    public decimal Cantidad { get; set; }

    public string? NombreProducto { get; set; }

    public decimal Costo { get; set; }
    public bool Activo { get; set; }

    public DetallePromocionVM() { }

    public DetallePromocionVM(DetallesPromociones detalle)
    {
        IdProducto = detalle.IdProducto;
        Cantidad = detalle.Cantidad;
        NombreProducto = detalle.Producto.Producto;
        Costo = detalle.Producto.Costo;
        Activo = detalle.Producto.Activo;
    }
    public decimal CalcularCosto()
    {
        return Cantidad*Costo;
    }
}