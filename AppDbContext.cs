using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace entornoPolleria
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Productos> Productos { get; set; }
        public DbSet<Proveedores> Proveedores { get; set; }
        public DbSet<Ventas> Ventas { get; set; }
        public DbSet<DetallesVentas> DetallesVentas { get; set; }
        public DbSet<MetodosDePago> MetodosDePago { get; set; }

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

            // Mantener compatibilidad con modelos existentes
            modelBuilder.UseSerialColumns();
            modelBuilder.HasPostgresExtension("pgcrypto");
        }
    }
} 