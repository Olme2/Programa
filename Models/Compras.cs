using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComprasVM;
using DetallesComprasVM;
public class Compras
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdCompra { get; set; }
    
    [ForeignKey("Proveedor")]
    public int IdProveedor { get; set; }
    
    public DateOnly Fecha { get; set; }
    public string? Detalle { get; set; }
    public readonly List<DetallesCompras> _detallesCompras = new();
    public virtual IReadOnlyCollection<DetallesCompras> DetallesCompra => _detallesCompras.AsReadOnly();
    public virtual Proveedores Proveedor { get; set; } = null!;
    private Compras() { }

    public Compras(int idProveedor, DateOnly fecha, string? detalle, List<DetallesCompras> detalles)
    {
        IdProveedor = idProveedor;
        Fecha = fecha;
        Detalle = detalle;
        _detallesCompras = detalles;
    }

    public Compras CrearDesdeViewModel(AltaCompraVM compraVM)
    {
        var detalles = compraVM.DetallesCompra.Select(DetallesCompras.CrearDesdeViewModel).ToList();
        return new Compras(compraVM.IdProveedor, compraVM.Fecha, compraVM.Detalle, detalles);
    }
    public void ActualizarDesdeViewModel(ModificarCompraVM compraVM)
    {
        IdProveedor = compraVM.IdProveedor;
        Fecha = compraVM.Fecha;
        Detalle = compraVM.Detalle;
        var detallesActualizados = compraVM.DetallesCompra.Select(DetallesCompras.CrearDesdeViewModel).ToList();
        LimpiarDetalles();
        foreach (var detalle in detallesActualizados)
        {
            AgregarDetalle(detalle);
        }
    }

    public void ActualizarDetalles(List<DetalleCompraVM> detallesVM)
    {
        var idsProductosEnViewModel = detallesVM.Select(d => d.IdProducto).ToList();
        var detallesAEliminar = _detallesCompras.Where(d => !idsProductosEnViewModel.Contains(d.IdProducto)).ToList();
        foreach (var detalle in detallesAEliminar)
        {
            _detallesCompras.Remove(detalle);
        }

        foreach (var detalleVM in detallesVM)
        {
            var detalleExistente = _detallesCompras.FirstOrDefault(d => d.IdProducto == detalleVM.IdProducto);
            if (detalleExistente != null)
            {
                detalleExistente.Cantidad = detalleVM.Cantidad;
            }
            else
            {
                _detallesCompras.Add(DetallesCompras.CrearDesdeViewModel(detalleVM));
            }
        }
    }

    public void LimpiarDetalles()
    {
        _detallesCompras.Clear();
    }

    public void AgregarDetalle(DetallesCompras detalle)
    {
        _detallesCompras.Add(detalle);
    }

    public decimal CalcularTotal()
    {
        return _detallesCompras.Sum(detalle => detalle.CalcularTotal());
    }
}