using System.ComponentModel.DataAnnotations;
namespace ProductosVM;
using ProveedoresVM;

public class ModificarProductoVM
    {
        [Required]
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string Producto { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
        [Display(Name = "Proveedor")]
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, 99999.999, ErrorMessage = "El stock no puede ser negativo ni mayor a 99.999,999.")]
        public decimal Stock { get; set; }

        [Required(ErrorMessage = "El costo es obligatorio.")]
        [Range(0.001, 99999.99, ErrorMessage = "El costo debe ser un valor positivo y no mayor a $99.999,99.")]
        public decimal Costo { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 99999.99, ErrorMessage = "El precio debe ser un valor positivo y no mayor a $99.999,99.")]
        public decimal Precio { get; set; }
        [Display(Name = "¿Está activo?")]
        public bool Activo { get; set; }
        public List<ListarProveedoresVM> Proveedores { get; set; }

        public ModificarProductoVM()
        {
            Producto = string.Empty;
            Proveedores = new List<ListarProveedoresVM>();
        }

        public ModificarProductoVM(Productos producto, List<ListarProveedoresVM> proveedores) : this()
        {
            IdProducto = producto.IdProducto;
            Producto = producto.Producto;
            IdProveedor = producto.IdProveedor;
            Stock = producto.Stock;
            Costo = producto.Costo;
            Precio = producto.Precio;
            Activo = producto.Activo;
            Proveedores = proveedores;
        }
    }