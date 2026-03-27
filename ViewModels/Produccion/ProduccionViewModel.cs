namespace ProduccionVM;

using System.ComponentModel.DataAnnotations;

public class AltaProduccionVM
{
    [Display(Name = "Fecha")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    public string? Detalle { get; set; }

    public List<DetalleProduccionVM> Detalles { get; set; } = new() { new DetalleProduccionVM() };

    public List<ProductosVM.ListarProductosVM> ProductosActivos { get; set; } = new();
}

public class DetalleProduccionVM
{
    public int IdProducto { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
}

public class ListarProduccionVM
{
    public int IdProduccion { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public string? Detalle { get; set; }
    public string Productos { get; set; } = string.Empty;
}

public class IndexProduccionVM
{
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(-30);

    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; } = DateTime.Today;

    public List<ListarProduccionVM> Producciones { get; set; } = new();
}
