using GastosVM;
using entornoPolleria;
using Microsoft.EntityFrameworkCore;
using VM = DashboardVM;

public interface IDashboardRepository
{
    VM.DashboardVM ObtenerDashboard(DateTime fechaInicio, DateTime fechaFin, string periodo);
}

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;
    public DashboardRepository(AppDbContext context) => _context = context;

    public VM.DashboardVM ObtenerDashboard(DateTime fechaInicio, DateTime fechaFin, string periodo)
    {
        var inicio = DateOnly.FromDateTime(fechaInicio);
        var fin    = DateOnly.FromDateTime(fechaFin);

        // ── 1. Ventas reales ─────────────────────────────────────────────
        var ventas = _context.Ventas
            .Include(v => v.DetallesVenta)
            .Include(v => v.VentaPromociones)
            .Where(v => v.Tipo == "venta" && v.Fecha >= inicio && v.Fecha <= fin)
            .AsEnumerable()
            .ToList();

        var totalVenta       = ventas.Sum(v => v.CalcularPrecioTotal());
        var totalCostoVentas = ventas.Sum(v => v.CalcularCostoTotal());

        // ── 2. Consumos ──────────────────────────────────────────────────
        var consumos = _context.Ventas
            .Include(v => v.DetallesVenta)
            .Where(v => v.Tipo == "consumo" && v.Fecha >= inicio && v.Fecha <= fin)
            .AsEnumerable()
            .ToList();

        var totalCostoConsumo = consumos.Sum(v => v.CalcularCostoTotal());

        // ── 3. Producciones ──────────────────────────────────────────────
        var producciones = _context.Ventas
            .Include(v => v.DetallesVenta)
            .Where(v => v.Tipo == "produccion" && v.Fecha >= inicio && v.Fecha <= fin)
            .AsEnumerable()
            .ToList();

        var totalCostoProduccion = producciones.Sum(v => v.CalcularCostoTotal());

        // ── 4. Gastos ────────────────────────────────────────────────────
        var gastosList = _context.Gastos
            .Where(g => g.Fecha >= inicio && g.Fecha <= fin)
            .OrderByDescending(g => g.Fecha)
            .Select(g => new ListarGastoVM { IdGasto = g.IdGasto, Fecha = g.Fecha, Nombre = g.Nombre, Monto = g.Monto, Observacion = g.Observacion })
            .ToList();

        var totalGastos = gastosList.Sum(g => g.Monto);

        // ── 5. Serie histórica para gráfico ─────────────────────────────
        var dias = (fechaFin - fechaInicio).Days;
        List<VM.PuntoTemporalVM> serie;

        if (dias <= 31)
        {
            // Agrupar por día
            serie = Enumerable.Range(0, dias + 1)
                .Select(d => fechaInicio.AddDays(d))
                .Select(dia => {
                    var diaOnly = DateOnly.FromDateTime(dia);
                    var vDia = ventas.Where(v => v.Fecha == diaOnly);
                    var cDia = consumos.Where(v => v.Fecha == diaOnly).Sum(v => v.CalcularCostoTotal());
                    var pDia = producciones.Where(v => v.Fecha == diaOnly).Sum(v => v.CalcularCostoTotal());
                    var gDia = gastosList.Where(g => g.Fecha == diaOnly).Sum(g => g.Monto);
                    var venta = vDia.Sum(v => v.CalcularPrecioTotal());
                    var costo = vDia.Sum(v => v.CalcularCostoTotal()) + cDia + pDia + gDia;
                    return new VM.PuntoTemporalVM { Label = dia.ToString("dd/MM"), Venta = venta, Costo = costo };
                })
                .ToList();
        }
        else if (dias <= 180)
        {
            // Agrupar por semana
            var firstMonday = fechaInicio.AddDays(-(int)fechaInicio.DayOfWeek + 1);
            var semanas = new List<VM.PuntoTemporalVM>();
            var cur = firstMonday;
            while (cur <= fechaFin)
            {
                var semI = DateOnly.FromDateTime(cur);
                var semF = DateOnly.FromDateTime(cur.AddDays(6));
                var vSem = ventas.Where(v => v.Fecha >= semI && v.Fecha <= semF);
                var cSem = consumos.Where(v => v.Fecha >= semI && v.Fecha <= semF).Sum(v => v.CalcularCostoTotal());
                var pSem = producciones.Where(v => v.Fecha >= semI && v.Fecha <= semF).Sum(v => v.CalcularCostoTotal());
                var gSem = gastosList.Where(g => g.Fecha >= semI && g.Fecha <= semF).Sum(g => g.Monto);
                var venta = vSem.Sum(v => v.CalcularPrecioTotal());
                var costo = vSem.Sum(v => v.CalcularCostoTotal()) + cSem + pSem + gSem;
                semanas.Add(new VM.PuntoTemporalVM { Label = cur.ToString("dd/MM"), Venta = venta, Costo = costo });
                cur = cur.AddDays(7);
            }
            serie = semanas;
        }
        else
        {
            // Agrupar por mes
            var meses = new List<VM.PuntoTemporalVM>();
            var cur = new DateTime(fechaInicio.Year, fechaInicio.Month, 1);
            while (cur <= fechaFin)
            {
                var mesI = DateOnly.FromDateTime(cur);
                var mesF = DateOnly.FromDateTime(cur.AddMonths(1).AddDays(-1));
                var vMes = ventas.Where(v => v.Fecha >= mesI && v.Fecha <= mesF);
                var cMes = consumos.Where(v => v.Fecha >= mesI && v.Fecha <= mesF).Sum(v => v.CalcularCostoTotal());
                var pMes = producciones.Where(v => v.Fecha >= mesI && v.Fecha <= mesF).Sum(v => v.CalcularCostoTotal());
                var gMes = gastosList.Where(g => g.Fecha >= mesI && g.Fecha <= mesF).Sum(g => g.Monto);
                var venta = vMes.Sum(v => v.CalcularPrecioTotal());
                var costo = vMes.Sum(v => v.CalcularCostoTotal()) + cMes + pMes + gMes;
                meses.Add(new VM.PuntoTemporalVM { Label = cur.ToString("MMM yy"), Venta = venta, Costo = costo });
                cur = cur.AddMonths(1);
            }
            serie = meses;
        }

        // ── 6. Top 5 productos por ingresos ─────────────────────────────
        var topProductos = _context.Ventas
            .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
            .Where(v => v.Tipo == "venta" && v.Fecha >= inicio && v.Fecha <= fin)
            .AsEnumerable()
            .SelectMany(v => v.DetallesVenta)
            .GroupBy(d => d.Producto.Producto)
            .Select(g => new VM.TopProductoVM
            {
                Producto   = g.Key,
                Cantidad   = g.Sum(d => d.Cantidad),
                TotalVenta = g.Sum(d => d.PrecioUnitario * d.Cantidad),
            })
            .OrderByDescending(x => x.TotalVenta)
            .Take(5)
            .ToList();

        // ── 7. Distribución de costos ─────────────────────────────────────
        var distribucion = new List<VM.DistribucionVM>
        {
            new() { Etiqueta = "Costo de Ventas",     Monto = totalCostoVentas },
            new() { Etiqueta = "Consumo Interno",      Monto = totalCostoConsumo },
            new() { Etiqueta = "Producción",           Monto = totalCostoProduccion },
            new() { Etiqueta = "Gastos Extras",        Monto = totalGastos },
        }.Where(d => d.Monto > 0).ToList();

        return new VM.DashboardVM
        {
            Periodo              = periodo,
            FechaInicio          = fechaInicio,
            FechaFin             = fechaFin,
            TotalVenta           = totalVenta,
            TotalCostoVentas     = totalCostoVentas,
            TotalCostoConsumo    = totalCostoConsumo,
            TotalCostoProduccion = totalCostoProduccion,
            TotalGastos          = totalGastos,
            CantidadVentas       = ventas.Count,
            CantidadConsumos     = consumos.Count,
            CantidadProducciones = producciones.Count,
            SerieHistorica       = serie,
            DistribucionCosto    = distribucion,
            TopProductos         = topProductos,
            DetalleGastos        = gastosList,
        };
    }
}
