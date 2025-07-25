using System.ComponentModel.DataAnnotations;
namespace ProductosVM;

public class ListarProductosVM
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public decimal Stock { get; set; }
        public decimal Costo { get; set; }        
        public decimal Precio { get; set; }

        public bool Activo { get; set; }
        [DataType(DataType.Currency)]
        public decimal Ganancia { get; set; }

        [Display(Name = "% Ganancia")]
        public decimal PorcentajeGanancia { get; set; }
        public bool EsEliminable { get; set; }
        public decimal VentaSemanal { get; set; }
        public ListarProductosVM() { }

        public ListarProductosVM(Productos producto)
        {
            IdProducto = producto.IdProducto;
            Producto = producto.Producto;
            Proveedor = producto.Proveedor?.Proveedor ?? "N/A"; 
            Stock = producto.Stock;
            Costo = producto.Costo;
            Precio = producto.Precio;
            Activo = producto.Activo;
            Ganancia = producto.CalcularGanancia();
            PorcentajeGanancia = producto.CalcularPorcentajeGanancia();
        }
    }
