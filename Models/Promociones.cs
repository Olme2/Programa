public class Promociones
{
    private int idPromocion;
    private string promocion;
    private decimal costo;
    private decimal precio;
    private decimal ganancia;
    private decimal porcentajeGanancia;
    private DateOnly inicio;
    private DateOnly? fin;

    public int IdPromocion { get => idPromocion; set => idPromocion = value; }
    public string Promocion { get => promocion; set => promocion = value; }
    public decimal Costo { get => costo; set => costo = value; }
    public decimal Precio { get => precio; set => precio = value; }
    public decimal Ganancia { get => ganancia; set => ganancia = value; }
    public decimal PorcentajeGanancia { get => porcentajeGanancia; set => porcentajeGanancia = value; }
    public DateOnly Inicio { get => inicio; set => inicio = value; }
    public DateOnly? Fin { get => fin; set => fin = value; }
}