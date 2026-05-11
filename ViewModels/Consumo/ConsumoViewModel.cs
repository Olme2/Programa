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
    public string NombreProducto { get; set; } = string.Empty;

    public DetalleConsumoVM() { }

    public DetalleConsumoVM(DetallesVentas detalle)
    {
        IdProducto = detalle.IdProducto;
        Cantidad = detalle.Cantidad;
        CostoUnitario = detalle.CostoUnitario;
        NombreProducto = $"{detalle.Producto.Producto} ($ {detalle.CostoUnitario.ToString("N2", CG.CulturaES)}) - S: {detalle.Producto.Stock.ToString("N3", CG.CulturaES)}";
    }
}

public class ModificarConsumoVM
{
    public int IdConsumo { get; set; }

    [Display(Name = "Fecha")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Display(Name = "Hora")]
    [DataType(DataType.Time)]
    public TimeOnly Hora { get; set; }

    public string? Detalle { get; set; }

    public List<DetalleConsumoVM> Detalles { get; set; } = new();

    public ModificarConsumoVM() { }

    public ModificarConsumoVM(Ventas consumo)
    {
        IdConsumo = (int)consumo.IdVenta;
        Fecha = consumo.Fecha.ToDateTime(TimeOnly.MinValue);
        Hora = consumo.Hora;
        Detalle = consumo.Detalle;
        Detalles = consumo.DetallesVenta.Select(d => new DetalleConsumoVM(d)).ToList();
    }
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
