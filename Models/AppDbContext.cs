using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Entidad> Entidades { get; set; }
    public DbSet<Beneficiario> Beneficiarios { get; set; }
    public DbSet<Subsidio> Subsidios { get; set; }
    public DbSet<Notificacion> Notificaciones { get; set; }
}