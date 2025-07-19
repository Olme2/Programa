using ProductosVM;

public class Productos
{
    public int IdProducto { get; private set; }
    public int IdProveedor { get; private set; }
    public string Producto { get; private set; }
    public decimal Stock { get; private set; }
    public decimal Costo { get; private set; }
    public decimal Precio { get; private set; }
    public bool Activo { get; private set; }
    
    public virtual Proveedores Proveedor { get; private set; }
    private Productos()
    {
        Producto = string.Empty;
        Proveedor = null!;
    }
    private Productos(int idProveedor, string nombre, decimal stock, decimal costo, decimal precio)
    {
        IdProveedor = idProveedor;
        Producto = nombre;
        Stock = stock;
        Costo = costo;
        Precio = precio;
        Activo = true;
        Proveedor = null!;
    }

    public static Productos CrearDesdeViewModel(AltaProductoVM vm)
    {
        return new Productos(vm.IdProveedor, vm.Producto, vm.Stock, vm.Costo, vm.Precio);
    }

    public void ActualizarDesdeViewModel(ModificarProductoVM vm)
    {
        IdProveedor = vm.IdProveedor;
        Producto = vm.Producto;
        Stock = vm.Stock;
        Costo = vm.Costo;
        Precio = vm.Precio;
        if (vm.Activo)
        {
            Activar();
        }
        else
        {
            Desactivar();
        }
    }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }
    public decimal CalcularGanancia()
    {
        return Precio - Costo;
    }

    public decimal CalcularPorcentajeGanancia()
    {
        if (Costo <= 0)
        {
            return 0;
        }
        return CalcularGanancia() / Costo; 
    }
}