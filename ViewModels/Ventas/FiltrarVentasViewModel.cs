namespace VentasVM;

public class FiltrarVentasVM
{
    private DateOnly? fechaInicio;
    private DateOnly? fechaFin;
    public DateOnly? FechaInicio { get => fechaInicio; set => fechaInicio = value; }
    public DateOnly? FechaFin { get => fechaFin; set => fechaFin = value; }
}