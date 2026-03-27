public class Gasto
{
    public int IdGasto { get; set; }
    public DateOnly Fecha { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? Observacion { get; set; }

    private Gasto() { }

    public Gasto(DateOnly fecha, string nombre, decimal monto, string? observacion)
    {
        Fecha = fecha;
        Nombre = nombre;
        Monto = monto;
        Observacion = observacion;
    }
}
