using Microsoft.EntityFrameworkCore;
using PromocionesVM;

namespace entornoPolleria.Repositorios
{
    public class VentaRepository : IVentaRepository
    {
        private readonly AppDbContext _context;

        public VentaRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ListarVentasVM> ObtenerVentasPorFechas(DateOnly inicio, DateOnly fin)
        {
            return _context.Ventas
                .Where(v => v.Fecha >= inicio && v.Fecha <= fin)
                .Include(v => v.Metodo)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(dv => dv.Producto)
                .Include(v => v.VentaPromociones)
                    .ThenInclude(vp => vp.Promocion)
                .Select(v => new ListarVentasVM()
                {
                    IdVenta = v.IdVenta,
                    Metodo = v.Metodo.Metodo,
                    ProductosYPromociones = string.Concat(string.Join("\n", v.VentaPromociones.Select(vp => vp.Promocion.Promocion)), "\n",string.Join("\n", v.DetallesVenta.Select(dv => dv.Producto.Producto))),
                    Precio = v.CalcularPrecioTotal(),
                    Fecha = v.Fecha,
                    Hora = v.Hora
                })
                .OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Hora)
                .ToList();
        }

        public Ventas? ObtenerVentaPorId(int id)
        {
            return _context.Ventas
                .Include(v => v.DetallesVenta)
                    .ThenInclude(dv => dv.Producto)
                .Include(v => v.VentaPromociones)
                    .ThenInclude(vp => vp.Promocion)
                        .ThenInclude(p => p.DetallesPromocion)
                            .ThenInclude(dp => dp.Producto)
                .FirstOrDefault(v => v.IdVenta == id);
        }

        public void CrearVenta(Ventas nuevaVenta)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Paso 1: Validar stock antes de hacer nada.
                ValidarStockParaVenta(nuevaVenta);

                // Paso 2: Añadir la venta al contexto.
                // EF Core se encargará de añadir los detalles en cascada.
                _context.Ventas.Add(nuevaVenta);

                // Paso 3: Guardar cambios. Los triggers de la BD se ejecutarán aquí.
                _context.SaveChanges();

                // Paso 4: Si todo fue bien, confirmar la transacción.
                transaction.Commit();
            }
            catch (Exception)
            {
                // Si algo falla (ej. validación o trigger), revertir todo.
                transaction.Rollback();
                throw; // Relanzar la excepción para que el controlador la maneje.
            }
        }

        public void ActualizarVenta(Ventas ventaModificada)
        {
            // La lógica de actualización es compleja y requiere un manejo cuidadoso
            // de las entidades para evitar conflictos con el tracking de EF Core.
            // Por ahora, implementamos la estructura y la lógica se puede añadir después.
            _context.Ventas.Update(ventaModificada);
            _context.SaveChanges();
        }

        public void EliminarVenta(int id)
        {
            var venta = _context.Ventas.Find(id);
            if (venta != null)
            {
                // Gracias al borrado en cascada, al eliminar la venta,
                // EF Core y la BD eliminarán los DetallesVenta y VentaPromociones asociados.
                // Los triggers de la BD se encargarán de devolver el stock.
                _context.Ventas.Remove(venta);
                _context.SaveChanges();
            }
        }

        // --- Métodos Privados de Ayuda ---

        private void ValidarStockParaVenta(Ventas venta)
        {
            // Validar productos individuales
            foreach (var detalle in venta.DetallesVenta)
            {
                var producto = _context.Productos.Find(detalle.IdProducto);
                if (producto == null || producto.Stock < detalle.Cantidad)
                {
                    throw new InvalidOperationException($"Stock insuficiente para el producto '{producto?.Producto ?? "ID: " + detalle.IdProducto}'. Stock disponible: {producto?.Stock ?? 0}.");
                }
            }

            // Validar productos dentro de promociones
            foreach (var ventaPromo in venta.VentaPromociones)
            {
                // Necesitamos cargar la promoción y sus detalles desde la BD
                var promocion = _context.Promociones
                    .Include(p => p.DetallesPromocion)
                    .AsNoTracking() // Usamos AsNoTracking para no interferir con el contexto principal
                    .FirstOrDefault(p => p.IdPromocion == ventaPromo.IdPromocion);

                if (promocion == null) continue;

                foreach (var detallePromo in promocion.DetallesPromocion)
                {
                    var producto = _context.Productos.Find(detallePromo.IdProducto);
                    var cantidadRequerida = ventaPromo.Cantidad * detallePromo.Cantidad;
                    if (producto == null || producto.Stock < cantidadRequerida)
                    {
                        throw new InvalidOperationException($"Stock insuficiente para el producto '{producto?.Producto ?? "ID: " + detallePromo.IdProducto}' dentro de la promoción '{promocion.Promocion}'. Stock disponible: {producto?.Stock ?? 0}, requerido: {cantidadRequerida}.");
                    }
                }
            }
        }
    }
}