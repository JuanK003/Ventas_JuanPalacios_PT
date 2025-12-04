using Microsoft.EntityFrameworkCore;
using VentasApi.Models;

namespace VentasApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Venta> Ventas { get; set; } = null!;
        public DbSet<DetalleVenta> DetalleVentas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Producto>().Property(p => p.Precio).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<DetalleVenta>().Property(d => d.PrecioUnitario).HasColumnType("decimal(18,2)");
        }
    }
}
