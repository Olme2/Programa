namespace DashboardVM;
using GastosVM;


public class PuntoTemporalVM
{
    public string Label { get; set; } = string.Empty; // "01/03", "Semana 12", etc.
    public decimal Venta { get; set; }
    public decimal Costo { get; set; }
    public decimal Ganancia => Venta - Costo;
}

public class DistribucionVM
{
    public string Etiqueta { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}

public class TopProductoVM
{
    public string Producto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal TotalVenta { get; set; }
}

public class DashboardVM
{
    // Período seleccionado
    public string Periodo { get; set; } = "mes";
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime FechaFin { get; set; } = DateTime.Today;

    // KPIs brutos
    public decimal TotalVenta { get; set; }
    public decimal TotalCostoVentas { get; set; }
    public decimal TotalCostoConsumo { get; set; }
    public decimal TotalCostoProduccion { get; set; }
    public decimal TotalGastos { get; set; }
    public int CantidadVentas { get; set; }
    public int CantidadConsumos { get; set; }
    public int CantidadProducciones { get; set; }

    // Calculados
    public decimal CostoTotal => TotalCostoVentas + TotalCostoConsumo + TotalCostoProduccion + TotalGastos;
    public decimal Ganancia => TotalVenta - CostoTotal;
    public decimal MargenPct => TotalVenta > 0 ? Math.Round(Ganancia / TotalVenta * 100, 1) : 0;
    public decimal CostoPct => TotalVenta > 0 ? Math.Round(CostoTotal / TotalVenta * 100, 1) : 0;

    // Para gráficos
    public List<PuntoTemporalVM> SerieHistorica { get; set; } = new();
    public List<DistribucionVM> DistribucionCosto { get; set; } = new();
    public List<TopProductoVM> TopProductos { get; set; } = new();

    // Detalle de gastos del período
    public List<GastosVM.ListarGastoVM> DetalleGastos { get; set; } = new();
}
