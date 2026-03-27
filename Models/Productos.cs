using ProductosVM;

public class Productos
{
    public int IdProducto { get; set; }
    public int IdProveedor { get; set; }
    public string Producto { get; set; }
    public decimal Stock { get; set; }
    public decimal Costo { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
    /// <summary>Costo que se activará cuando el stock baje del umbral (gestionado por el trigger de Postgres).</summary>
    public decimal? CostoPendiente { get; set; }
    /// <summary>Nivel de stock al que se activa CostoPendiente.</summary>
    public decimal? StockUmbral { get; set; }

    public virtual Proveedores Proveedor { get; set; }

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