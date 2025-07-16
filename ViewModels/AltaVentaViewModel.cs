using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class AltaVentaViewModel
{
    [Required(ErrorMessage = "Debe seleccionar al menos un producto")] 
    public List<DetalleVentaViewModel> Detalles { get; set; } = new();

    [Required(ErrorMessage = "Debe seleccionar un método de pago")]
    [Display(Name = "Método de Pago")]
    public short? IdMetodo { get; set; }

    public List<MetodosDePago>? MetodosDePago { get; set; }
    public string? Detalle { get; set; }
}

public class DetalleVentaViewModel
{
    [Required]
    public int IdProducto { get; set; }
    [Required]
    [Range(0.001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal Cantidad { get; set; }
    public bool Promocion { get; set; }
    public decimal PrecioPromo { get; set; }
    public decimal CostoPromo { get; set; }
    public string? NombreProducto { get; set; }
    public decimal StockDisponible { get; set; }
} 