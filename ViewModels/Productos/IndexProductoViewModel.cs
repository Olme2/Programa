
namespace ProductosVM;

public class IndexProductosVM
{
    // --- Filtros que SÍ existen ---
    public string? Busqueda { get; set; }
    public string OrdenarPor { get; set; }
    public bool Inactivos { get; set; }

    // --- Resultados ---
    public List<ListarProductosVM> Productos { get; set; }

    public IndexProductosVM()
    {
        // Valor por defecto para el orden, igual que en tu vista
        OrdenarPor = "alfabetico";
        Productos = new List<ListarProductosVM>();
    }
}