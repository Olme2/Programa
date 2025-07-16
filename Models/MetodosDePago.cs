public class MetodosDePago
{
    private short idMetodo;
    private string metodo;
    public short IdMetodo { get => idMetodo; set => idMetodo = value; }
    public string Metodo { get => metodo; set => metodo = value; }
    public MetodosDePago() {}
    public MetodosDePago(short idMetodo, string metodo)
    {
        IdMetodo = idMetodo;
        Metodo = metodo;
    }
}