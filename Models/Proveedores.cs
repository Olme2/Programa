using ProveedoresVM;

public class Proveedores
{
    public int IdProveedor { get; private set; }
    public string Proveedor { get; private set; }
    public string? Contacto { get; private set; }
    public decimal Debo { get; private set; }
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

    public static Proveedores CrearDesdeViewModel(AltaProveedorVM vm)
    {
        return new Proveedores(vm.Proveedor, vm.Contacto, vm.Debo);
    }

    public void ActualizarDesdeViewModel(ModificarProveedorVM vm)
    {
        Proveedor = vm.Proveedor;
        Contacto = vm.Contacto;
        Debo = vm.Debo;
    }
}