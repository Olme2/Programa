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

    // --- CONSTRUCTORES ---
    private Productos()
    {
        Producto = string.Empty;
        Proveedor = null!; // Le decimos al compilador que EF se encargará de esto.
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

    public static Productos CrearDesdeViewModel(AltaProductoVM productoVM)
    {
        return new Productos(productoVM.IdProveedor, productoVM.Producto, productoVM.Stock, productoVM.Costo, productoVM.Precio);
    }

    public void ActualizarDesdeViewModel(ModificarProductoVM productoVM)
    {
        IdProveedor = productoVM.IdProveedor;
        Producto = productoVM.Producto;
        Stock = productoVM.Stock;
        Costo = productoVM.Costo;
        Precio = productoVM.Precio;
        if (productoVM.Activo)
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