using System.Collections.Generic;

public class VentaConDetallesViewModel
{
    public long IdVenta { get; set; }
    public string MetodoDePago { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Costo { get; set; }
    public decimal Ganancia { get; set; }
    public decimal PorcentajeGanancia { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public string? Detalle { get; set; }
    public List<DetalleVentaViewModel> Detalles { get; set; } = new();
} 