namespace MetodosVM;

public class ListarMetodosDePagoVM
{
    private short idMetodo;
    private string metodo;

    public ListarMetodosDePagoVM()
    {
        metodo = string.Empty;
    }
    public ListarMetodosDePagoVM(MetodosDePago Metodo)
    {
        idMetodo = Metodo.IdMetodo;
        metodo = Metodo.Metodo;
    }

    public short IdMetodo { get => idMetodo; set => idMetodo = value; }
    public string Metodo { get => metodo; set => metodo = value; }
}