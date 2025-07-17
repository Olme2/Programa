namespace MetodosVM;

public class ModificarMetodoDePagoVM
{
    private short idMetodo;
    private string metodo;

    public ModificarMetodoDePagoVM()
    {
        metodo = string.Empty;
    }
    public ModificarMetodoDePagoVM(MetodosDePago Metodo)
    {
        idMetodo = Metodo.IdMetodo;
        metodo = Metodo.Metodo;
    }

    public short IdMetodo { get => idMetodo; set => idMetodo = value; }
    public string Metodo { get => metodo; set => metodo = value; }
}