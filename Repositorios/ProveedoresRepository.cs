using entornoPolleria;
using ProveedoresVM;
public class ProveedoresRepository : IProveedoresRepository
{
    private readonly AppDbContext _context;
    
    public ProveedoresRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public IEnumerable<ListarProveedoresVM> ObtenerListadoProveedores()
    {
        // Proyectamos directamente a nuestro ViewModel
        return _context.Proveedores
            .Select(p => new ListarProveedoresVM
            {
                IdProveedor = p.IdProveedor,
                Proveedor = p.Proveedor,
                Contacto = p.Contacto,
                Debo = p.Debo,
                // Calculamos si el proveedor está referenciado en la tabla de productos.
                EsEliminable = !_context.Productos.Any(prod => prod.IdProveedor == p.IdProveedor)
            })
            .ToList();
    }

    public Proveedores? ObtenerPorId(int id)
    {
        return _context.Proveedores.FirstOrDefault(p => p.IdProveedor == id);
    }

    public void Crear(Proveedores proveedor)
    {
        _context.Proveedores.Add(proveedor);
        _context.SaveChanges();
    }

    public void Actualizar(Proveedores proveedor)
    {
        _context.Proveedores.Update(proveedor);
        _context.SaveChanges();
    }

    public void Eliminar(int id)
    {
        var proveedor = _context.Proveedores.Find(id);
        if (proveedor != null)
        {
            _context.Proveedores.Remove(proveedor);
            _context.SaveChanges();
        }
    }
}