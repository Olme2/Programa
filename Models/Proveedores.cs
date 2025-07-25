using ProveedoresVM;

public class Proveedores
{
    public int IdProveedor { get; set; }
    public string Proveedor { get; set; }
    public string? Contacto { get; set; }
    public decimal Debo { get; set; }
    public virtual ICollection<Productos> Productos { get; private set; }
    private Proveedores()
    {
        Proveedor = string.Empty;
        Productos = new List<Productos>();
    }

    private Proveedores(string proveedor, string? contacto, decimal deboInicial)
    {
        Proveedor = proveedor;
        Contacto = contacto;
        Debo = deboInicial;
        Productos = null!;
    }

    public static Proveedores CrearDesdeViewModel(AltaProveedorVM proveedorVM)
    {
        return new Proveedores(proveedorVM.Proveedor, proveedorVM.Contacto, proveedorVM.Debo);
    }

    public void ActualizarDesdeViewModel(ModificarProveedorVM proveedorVM)
    {
        Proveedor = proveedorVM.Proveedor;
        Contacto = proveedorVM.Contacto;
        Debo = proveedorVM.Debo;
    }
}