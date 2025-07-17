using entornoPolleria;

class ProveedoresRepository : IProveedoresRepository
{
    private readonly AppDbContext _context;
    
    public ProveedoresRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public List<Proveedores> ListarProveedores()
    {
        return _context.Set<Proveedores>().ToList();
    }

    public void CrearNuevoProveedor(Proveedores proveedor)
    {
        _context.Set<Proveedores>().Add(proveedor);
        _context.SaveChanges();
    }

    public void ModificarProveedor(Proveedores proveedor)
    {
        _context.Set<Proveedores>().Update(proveedor);
        _context.SaveChanges();
    }

    public Proveedores ObtenerDetallesDeProveedorPorId(int id)
    {
        var proveedor = _context.Set<Proveedores>().FirstOrDefault(p => p.IdProveedor == id);
        if (proveedor == null)
        {
            throw new Exception("Proveedor inexistente");
        }
        return proveedor;
    }

    public void EliminarProveedorPorId(int id)
    {
        var proveedor = _context.Set<Proveedores>().FirstOrDefault(p => p.IdProveedor == id);
        if (proveedor != null)
        {
            _context.Set<Proveedores>().Remove(proveedor);
            _context.SaveChanges();
        }
    }
}
