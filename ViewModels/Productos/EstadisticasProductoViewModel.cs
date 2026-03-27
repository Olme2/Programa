namespace ProductosVM;

public class EstadisticasProductoVM
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string NombreProveedor { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime FechaFin { get; set; } = DateTime.Today;

    public List<FilaEstadisticaVM> Filas { get; set; } = new();

    public decimal TotalCantidad => Filas.Sum(f => f.Cantidad);
}

public class FilaEstadisticaVM
{
    public DateOnly Fecha     { get; set; }
    public TimeOnly Hora      { get; set; }
    public decimal  Cantidad  { get; set; }
    public string   Origen    { get; set; } = "Venta directa"; // o nombre de la promo
}
