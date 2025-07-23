using MetodosVM;

public class MetodosPago
{
    public short IdMetodo { get ; private set; }
    public string Metodo { get ; private set; }
    public virtual ICollection<Ventas> Ventas { get; private set; }
    private MetodosPago()
    {
        Metodo = string.Empty;
        Ventas = new List<Ventas>();
    }

    private MetodosPago(string metodo)
    {
        Metodo = metodo;
        Ventas = null!;
    }
    public static MetodosPago CrearDesdeViewModel(AltaMetodoPagoVM metodoVM)
    {
        return new MetodosPago(metodoVM.Metodo);
    }
    
    public void ActualizarDesdeViewModel(ModificarMetodoPago metodoVM)
    {
        IdMetodo = metodoVM.IdMetodo;
        Metodo = metodoVM.Metodo;
    }

    
}