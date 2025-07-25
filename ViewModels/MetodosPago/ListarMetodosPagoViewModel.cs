namespace MetodosVM;

public class ListarMetodosPagoVM
{
    public short IdMetodo { get ; set; }
    public string Metodo { get ; set; } = string.Empty;
    public bool EsEliminable { get; set; }

    public ListarMetodosPagoVM(){}
}