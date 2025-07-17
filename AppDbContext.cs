using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace entornoPolleria
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Productos> Productos { get; set; }
        public DbSet<Proveedores> Proveedores { get; set; }
        public DbSet<MetodosDePago> MetodosDePago { get; set; } // Agregado para métodos de pago
        public DbSet<Ventas> Ventas { get; set; } // Agregado para ventas
        public DbSet<DetallesVentas> DetallesVentas { get; set; } // Agregado para detalles de venta

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .AddIniFile("postgresql.conf")
                    .Build();
                optionsBuilder.UseNpgsql($"Host={config["Host"]};Port={config["Port"]};Database={config["Database"]};Username={config["Username"]};Password={config["Password"]}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración del modelo Productos
            modelBuilder.Entity<Productos>(entity =>
            {
                entity.ToTable("producto"); // Mapear a la tabla 'producto'
                entity.HasKey(e => e.IdProducto);
                entity.Property(e => e.IdProducto).HasColumnName("id_producto").UseIdentityColumn();
                entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor").IsRequired();
                entity.Property(e => e.Producto).HasColumnName("producto").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Stock).HasColumnName("stock").HasColumnType("numeric(6,3)").IsRequired();
                entity.Property(e => e.Costo).HasColumnName("costo_producto").HasColumnType("numeric(7,2)").IsRequired();
                entity.Property(e => e.Precio).HasColumnName("precio_producto").HasColumnType("numeric(7,2)").IsRequired();
                entity.Property(e => e.Ganancia).HasColumnName("ganancia_producto").HasColumnType("numeric(7,2)").ValueGeneratedOnAddOrUpdate().HasDefaultValueSql("precio_producto - costo_producto");
                entity.Property(e => e.PorcentajeGanancia).HasColumnName("porcentaje_ganancia_p").HasColumnType("numeric(5,2)").ValueGeneratedOnAddOrUpdate().HasDefaultValueSql("((precio_producto - costo_producto) / costo_producto) * 100");
            });

            // Configuración del modelo Proveedores
            modelBuilder.Entity<Proveedores>(entity =>
            {
                entity.ToTable("proveedor");
                entity.HasKey(e => e.IdProveedor);
                entity.Property(e => e.IdProveedor).HasColumnName("id_proveedor").UseIdentityColumn();
                entity.Property(e => e.Proveedor).HasColumnName("proveedor").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Contacto).HasColumnName("contacto").HasMaxLength(30);
            });

            // Configuración del modelo Ventas
            modelBuilder.Entity<Ventas>(entity =>
            {
                entity.ToTable("venta");
                entity.HasKey(e => e.IdVenta);
                entity.Property(e => e.IdVenta).HasColumnName("id_venta").UseIdentityColumn();
                entity.Property(e => e.IdMetodo).HasColumnName("id_metodo").IsRequired();
                entity.Property(e => e.Total).HasColumnName("total").HasColumnType("numeric(8,2)").IsRequired();
                entity.Property(e => e.Costo).HasColumnName("costo_venta").HasColumnType("numeric(7,2)").IsRequired();
                entity.Property(e => e.Ganancia).HasColumnName("ganancia_venta").HasColumnType("numeric(7,2)").IsRequired();
                entity.Property(e => e.PorcentajeGanancia).HasColumnName("porcentaje_ganancia_v").HasColumnType("numeric(5,2)").IsRequired();
                entity.Property(e => e.Fecha).HasColumnName("fecha").IsRequired();
                entity.Property(e => e.Hora).HasColumnName("hora").IsRequired();
                entity.Property(e => e.Detalle).HasColumnName("detalle").HasMaxLength(100);
            });

            // Configuración del modelo DetallesVentas
            modelBuilder.Entity<DetallesVentas>(entity =>
            {
                entity.ToTable("detalle_venta");
                entity.HasKey(e => new { e.IdVenta, e.IdProducto });
                entity.Property(e => e.IdVenta).HasColumnName("id_venta").IsRequired();
                entity.Property(e => e.IdProducto).HasColumnName("id_producto").IsRequired();
                entity.Property(e => e.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(6,2)").IsRequired();
                entity.Property(e => e.Promocion).HasColumnName("promocion").IsRequired();
                entity.Property(e => e.PrecioPromo).HasColumnName("precio_promo").HasColumnType("numeric(7,2)").IsRequired();
                entity.Property(e => e.CostoPromo).HasColumnName("costo_promo").HasColumnType("numeric(7,2)").IsRequired();
            });

            // Configuración del modelo MetodosDePago
            modelBuilder.Entity<MetodosDePago>(entity =>
            {
                entity.ToTable("metodo_pago");
                entity.HasKey(e => e.IdMetodo);
                entity.Property(e => e.IdMetodo).HasColumnName("id_metodo").UseIdentityColumn();
                entity.Property(e => e.Metodo).HasColumnName("metodo").HasMaxLength(30).IsRequired();
            });

            // Mantener compatibilidad con modelos existentes
            modelBuilder.UseSerialColumns();
            modelBuilder.HasPostgresExtension("pgcrypto");
        }
    }
} 