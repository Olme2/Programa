using System.ComponentModel.DataAnnotations;
using System.Globalization;
using ProductosVM;
namespace ProveedoresVM;

public class ListarProveedoresVM
{
    public int IdProveedor { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    [DataType(DataType.Currency)]
    public decimal Debo { get; set; }
    public bool EsEliminable { get; set; }
    public string ProductosReferenciados { get; set; } = string.Empty;
    public ListarProveedoresVM() { }
    public string DeboFormateado
    {
        get
        {
            CultureInfo culturaArgentina = new CultureInfo("es-AR");
            return Debo.ToString("C",culturaArgentina);
        }
    }
}