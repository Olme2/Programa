using System.ComponentModel.DataAnnotations;
using ProveedoresVM;

namespace ComprasVM;

public class IndexComprasVM
{
    [Display(Name = "Proveedor")]
    public int? IdProveedor { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; } = DateTime.Today;

    public List<ListarComprasVM> Compras { get; set; } = new List<ListarComprasVM>();
    public List<ListarProveedoresVM> Proveedores { get; set; } = new List<ListarProveedoresVM>();

    public IndexComprasVM() { }
    public IndexComprasVM(DateTime inicio, DateTime fin, int? idProveedor)
    {
        FechaInicio = inicio;
        FechaFin = fin;
        IdProveedor = idProveedor;
    }
    public IndexComprasVM(IndexComprasVM filtro)
    {
        FechaInicio = filtro.FechaInicio == default ? DateTime.Today : filtro.FechaInicio;
        FechaFin = filtro.FechaFin == default ? DateTime.Today : filtro.FechaFin;
        IdProveedor = filtro.IdProveedor;
    }
}