using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace entornoPolleria
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Productos> Productos { get; set; }
        public DbSet<Proveedores> Proveedores { get; set; }
        public DbSet<MetodosPago> MetodosPago { get; set; }
        public DbSet<Ventas> Ventas { get; set; }
        public DbSet<DetallesVentas> DetallesVentas { get; set; }
        public DbSet<Compras> Compras { get; set; }
        public DbSet<DetallesCompras> DetallesCompras { get; set; }
        public DbSet<Promociones> Promociones { get; set; }
        public DbSet<DetallesPromociones> DetallesPromociones { get; set; }
        public DbSet<VentasPromociones> VentasPromociones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuración del modelo Productos
            modelBuilder.Entity<Productos>(entity =>
            {
                entity.ToTable("producto");
                entity.HasKey(e => e.IdProducto);
                entity.Property(e => e.IdProducto).HasColumnName("id_producto").UseIdentityColumn();
                entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor").IsRequired();
                entity.Property(e => e.Producto).HasColumnName("producto").HasMaxLength(75).IsRequired();
                entity.Property(e => e.Stock).HasColumnName("stock").HasColumnType("numeric(6,3)").IsRequired();
                entity.Property(e => e.Costo).HasColumnName("costo_producto").HasColumnType("numeric(7,2)").IsRequired();
                entity.Property(e => e.Precio).HasColumnName("precio_producto").HasColumnType("numeric(7,2)").IsRequired();
                entity.Property(e => e.Activo).HasColumnName("activo").HasColumnType("boolean").HasDefaultValue(true);
            });
            // Proveedores
            modelBuilder.Entity<Proveedores>(entity =>
            {
                entity.ToTable("proveedor");
                entity.HasKey(e => e.IdProveedor);
                entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor").UseIdentityColumn();
                entity.Property(e => e.Proveedor).HasColumnName("proveedor").HasMaxLength(75).IsRequired();
                entity.Property(e => e.Contacto).HasColumnName("contacto").HasMaxLength(100);
                entity.Property(e => e.Debo).HasColumnName("debo").HasColumnType("numeric(9,2)").IsRequired();
                entity.HasMany(p => p.Productos)
                        .WithOne(prod => prod.Proveedor) // Cada Producto tiene un Proveedor.
                        .HasForeignKey(prod => prod.IdProveedor) // La clave foránea está en Productos.
                        .IsRequired();
            });

            // Compras
            modelBuilder.Entity<Compras>(entity =>
            {
                entity.ToTable("compra");
                entity.HasKey(e => e.IdCompra);
                entity.Property(e => e.IdCompra).HasColumnName("id_compra").UseIdentityColumn();
                entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor").IsRequired();
                entity.Property(e => e.Total).HasColumnName("total").HasColumnType("numeric(9,2)").IsRequired();
                entity.Property(e => e.Fecha).HasColumnName("fecha").IsRequired();
                entity.Property(e => e.Detalle).HasColumnName("detalle").HasMaxLength(100);
            });

            // DetallesCompras
            modelBuilder.Entity<DetallesCompras>(entity =>
            {
                entity.ToTable("detalle_compra");
                entity.HasKey(e => new { e.IdCompra, e.IdProducto });
                entity.Property(e => e.IdCompra).HasColumnName("id_compra").IsRequired();
                entity.Property(e => e.IdProducto).HasColumnName("id_producto").IsRequired();
                entity.Property(e => e.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(6,3)").IsRequired();
                entity.Property(e => e.CostoUnitario).HasColumnName("costo_unitario").HasColumnType("numeric(7,2)").IsRequired();
            });

            // Promociones
            modelBuilder.Entity<Promociones>(entity =>
            {
                entity.ToTable("promocion");
                entity.HasKey(e => e.IdPromocion);
                entity.Property(e => e.IdPromocion).HasColumnName("id_promocion").UseIdentityColumn();
                entity.Property(e => e.Promocion).HasColumnName("promocion").HasMaxLength(75).IsRequired();
                entity.Property(e => e.Precio).HasColumnName("precio_promocion").HasColumnType("numeric(7,2)").IsRequired();
                entity.Property(e => e.Inicio).HasColumnName("inicio").IsRequired();
                entity.Property(e => e.Fin).HasColumnName("fin");
            });

            // DetallesPromociones
            modelBuilder.Entity<DetallesPromociones>(entity =>
            {
                entity.ToTable("detalle_promocion");
                entity.HasKey(e => new { e.IdProducto, e.IdPromocion });
                entity.Property(e => e.IdProducto).HasColumnName("id_producto").IsRequired();
                entity.Property(e => e.IdPromocion).HasColumnName("id_promocion").IsRequired();
                entity.Property(e => e.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(6,3)").IsRequired();
                entity.HasOne<Promociones>()
                      .WithMany(p => p.DetallesPromocion) 
                      .HasForeignKey(d => d.IdPromocion) 
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); 
            });

            // Ventas
            modelBuilder.Entity<Ventas>(entity =>
            {
                entity.ToTable("venta");
                entity.HasKey(e => e.IdVenta);
                entity.Property(e => e.IdVenta).HasColumnName("id_venta").UseIdentityColumn();
                entity.Property(e => e.IdMetodo).HasColumnName("id_metodo").IsRequired();
                entity.Property(e => e.Fecha).HasColumnName("fecha").IsRequired();
                entity.Property(e => e.Hora).HasColumnName("hora").IsRequired();
                entity.Property(e => e.Detalle).HasColumnName("detalle").HasMaxLength(100);
                entity.Property(e => e.Redondeo).HasColumnName("redondeo");
            });

            // DetallesVentas
            modelBuilder.Entity<DetallesVentas>(entity =>
            {
                entity.ToTable("detalle_venta");
                entity.HasKey(e => new { e.IdVenta, e.IdProducto });
                entity.Property(e => e.IdVenta).HasColumnName("id_venta").IsRequired();
                entity.Property(e => e.IdProducto).HasColumnName("id_producto").IsRequired();
                entity.Property(e => e.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(6,3)").IsRequired();
                entity.Property(e => e.CostoUnitario).HasColumnName("costo_unitario").HasColumnType("numeric(7,2)").IsRequired();
                entity.Property(e => e.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(7,2)").IsRequired();
                entity.HasOne<Ventas>()
                    .WithMany(v => v.DetallesVenta)
                    .HasForeignKey(d => d.IdVenta)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // VentasPromociones
            modelBuilder.Entity<VentasPromociones>(entity =>
            {
                entity.ToTable("venta_promocion");
                entity.HasKey(e => new { e.IdVenta, e.IdPromocion });
                entity.Property(e => e.IdVenta).HasColumnName("id_venta").IsRequired();
                entity.Property(e => e.IdPromocion).HasColumnName("id_promocion").IsRequired();
                entity.Property(e => e.Cantidad).HasColumnName("cantidad").HasColumnType("smallint").IsRequired();
                entity.Property(e => e.CostoPromo).HasColumnName("costo_promo").HasColumnType("numeric(7,2)").IsRequired();
                entity.Property(e => e.PrecioPromo).HasColumnName("precio_promo").HasColumnType("numeric(7,2)").IsRequired();
                entity.HasOne<Ventas>()
                    .WithMany(v => v.VentaPromociones)
                    .HasForeignKey(d => d.IdVenta)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MetodosDePago
            modelBuilder.Entity<MetodosPago>(entity =>
            {
                entity.ToTable("metodo_pago");
                entity.HasKey(e => e.IdMetodo);
                entity.Property(e => e.IdMetodo).HasColumnName("id_metodo").UseIdentityColumn();
                entity.Property(e => e.Metodo).HasColumnName("metodo").HasMaxLength(50).IsRequired();
                entity.HasMany(m => m.Ventas)
                        .WithOne(v => v.Metodo)
                        .HasForeignKey(v => v.IdMetodo)
                        .IsRequired();
            });

            modelBuilder.UseSerialColumns();
            modelBuilder.HasPostgresExtension("pgcrypto");
        }
    }
} 