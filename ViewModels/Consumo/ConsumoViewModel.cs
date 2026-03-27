namespace ConsumoVM;

using System.ComponentModel.DataAnnotations;

public class AltaConsumoVM
{
    [Display(Name = "Fecha")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    public string? Detalle { get; set; }

    public List<DetalleConsumoVM> Detalles { get; set; } = new() { new DetalleConsumoVM() };

    // Para el dropdown de Select2
    public List<ProductosVM.ListarProductosVM> ProductosActivos { get; set; } = new();
}

public class DetalleConsumoVM
{
    public int IdProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
}

public class ListarConsumoVM
{
    public int IdConsumo { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public string? Detalle { get; set; }
    public string Productos { get; set; } = string.Empty;
    public decimal TotalCosto { get; set; }
}

public class IndexConsumoVM
{
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(-30);

    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; } = DateTime.Today;

    public List<ListarConsumoVM> Consumos { get; set; } = new();
}
