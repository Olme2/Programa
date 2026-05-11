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
    public string NombreProducto { get; set; } = string.Empty;

    public DetalleProduccionVM() { }

    public DetalleProduccionVM(DetallesVentas detalle)
    {
        IdProducto = detalle.IdProducto;
        Cantidad = detalle.Cantidad;
        CostoUnitario = detalle.CostoUnitario;
        NombreProducto = $"{detalle.Producto.Producto} ($ {detalle.CostoUnitario.ToString("N2", CG.CulturaES)}) - S: {detalle.Producto.Stock.ToString("N3", CG.CulturaES)}";
    }
}

public class ModificarProduccionVM
{
    public int IdProduccion { get; set; }

    [Display(Name = "Fecha")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Display(Name = "Hora")]
    [DataType(DataType.Time)]
    public TimeOnly Hora { get; set; }

    public string? Detalle { get; set; }

    public List<DetalleProduccionVM> Detalles { get; set; } = new();

    public ModificarProduccionVM() { }

    public ModificarProduccionVM(Ventas produccion)
    {
        IdProduccion = (int)produccion.IdVenta;
        Fecha = produccion.Fecha.ToDateTime(TimeOnly.MinValue);
        Hora = produccion.Hora;
        Detalle = produccion.Detalle;
        Detalles = produccion.DetallesVenta.Select(d => new DetalleProduccionVM(d)).ToList();
    }
}

public class ListarProduccionVM
{
    public int IdProduccion { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public string? Detalle { get; set; }
    public string Productos { get; set; } = string.Empty;
    public decimal TotalCosto { get; set; }
}

public class IndexProduccionVM
{
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(-30);

    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; } = DateTime.Today;

    public List<ListarProduccionVM> Producciones { get; set; } = new();
}
