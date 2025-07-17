using MetodosVM;

public class MetodosDePago
{
    private short idMetodo;
    private string metodo;

    public MetodosDePago()
    {
        metodo = string.Empty;
    }

    public MetodosDePago(AltaMetodoDePagoVM MetodoVM)
    {
        metodo = MetodoVM.Metodo;
    }
    
    public MetodosDePago(ModificarMetodoDePagoVM MetodoVM)
    {
        idMetodo = MetodoVM.IdMetodo;
        metodo = MetodoVM.Metodo;
    }

    public short IdMetodo { get => idMetodo; set => idMetodo = value; }
    public string Metodo { get => metodo; set => metodo = value; }
}