using System.ComponentModel.DataAnnotations; 
public class ModificarPresupuestoViewModel{
    private int idPresupuesto;
    private int idCliente;
    private DateTime fechaCreacion;
    public ModificarPresupuestoViewModel(){
    }
    
    public ModificarPresupuestoViewModel(Presupuestos presupuesto){
        idPresupuesto = presupuesto.IdPresupuesto;
        fechaCreacion = presupuesto.FechaCreacion;
    }

    public int IdPresupuesto { get => idPresupuesto; set => idPresupuesto = value;}

    [Required(ErrorMessage = "Id del cliente obligatorio")]
    public int IdCliente { get => idCliente; set => idCliente = value;}

    [Required(ErrorMessage = "Fecha de creacion obligatoria")]
    public DateTime FechaCreacion { get => fechaCreacion; set => fechaCreacion = value;}
}