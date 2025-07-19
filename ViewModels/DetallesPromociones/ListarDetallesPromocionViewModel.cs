namespace DetallesPromocionesVM;

public class ListarDetallesPromocionVM
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public ListarDetallesPromocionVM() { }
        public ListarDetallesPromocionVM(DetallesPromociones detalle)
        {
            if (detalle.Producto != null)
            {
                IdProducto = detalle.Producto.IdProducto;
                NombreProducto = detalle.Producto.Producto;
                PrecioUnitario = detalle.Producto.Precio;
            }
            Cantidad = detalle.Cantidad;
            Subtotal = PrecioUnitario * Cantidad;
        }
    }