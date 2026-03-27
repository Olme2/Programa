namespace GastosVM;
using System.ComponentModel.DataAnnotations;

public class AltaGastoVM
{
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Concepto")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }

    public string? Observacion { get; set; }
}

public class ListarGastoVM
{
    public int IdGasto { get; set; }
    public DateOnly Fecha { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Observacion { get; set; }
}

public class IndexGastosVM
{
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(-30);

    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; } = DateTime.Today;

    public List<ListarGastoVM> Gastos { get; set; } = new();
    public decimal Total => Gastos.Sum(g => g.Monto);
}
