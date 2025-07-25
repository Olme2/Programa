using MetodosVM;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace VentasVM;

public class IndexVentasVM
{
    // Para mostrar la lista de ventas filtrada
    public List<ListarVentasVM> Ventas { get; set; } = new List<ListarVentasVM>();

    // Para llenar el dropdown de filtro de métodos de pago
    public List<ListarMetodosPagoVM> MetodosPago { get; set; } = new List<ListarMetodosPagoVM>();

    // Propiedades para mantener el estado de los filtros
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; } = DateTime.Today;
    public decimal Total { get; set; } = 0;
    public string? Busqueda { get; set; }

    [Display(Name = "Método de Pago")]
    public short? IdMetodoPago { get; set; }
    public IndexVentasVM() { }
    public IndexVentasVM(IndexVentasVM filtro)
    {
        FechaInicio = filtro.FechaInicio == default ? DateTime.Today : filtro.FechaInicio;
        FechaFin = filtro.FechaFin == default ? DateTime.Today : filtro.FechaFin;
        Busqueda = filtro.Busqueda;
        IdMetodoPago = filtro.IdMetodoPago;
    }
}