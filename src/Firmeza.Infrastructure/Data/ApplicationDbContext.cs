using Firmeza.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<UsuarioSistema>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Administrador> Administradores { get; set; } = null!;
    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<Venta> Ventas { get; set; } = null!;
    public DbSet<DetalleVenta> DetallesVentas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Mapeo Uno a Uno explícito para separar tablas de Identity
        builder.Entity<Cliente>()
            .HasOne(c => c.Usuario)
            .WithOne(u => u.PerfilCliente)
            .HasForeignKey<Cliente>(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Administrador>()
            .HasOne(a => a.Usuario)
            .WithOne(u => u.PerfilAdministrador)
            .HasForeignKey<Administrador>(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuración de precisión decimal indispensable para PostgreSQL
        builder.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);
        builder.Entity<Venta>().Property(v => v.Total).HasPrecision(18, 2);
        builder.Entity<Venta>().Property(v => v.Iva).HasPrecision(18, 2);
        builder.Entity<DetalleVenta>().Property(d => d.PrecioUnitario).HasPrecision(18, 2);
    }
}