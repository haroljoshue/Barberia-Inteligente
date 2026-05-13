using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;

namespace Barberia.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Barbero> Barberos { get; set; } = default!;
        public DbSet<Cita> Citas { get; set; } = default!;
        public DbSet<LogAgente> LogsAgente { get; set; } = default!;
        public DbSet<LogSistema> LogsSistema { get; set; } = default!;
        public DbSet<Servicio> Servicios { get; set; } = default!;
    }
}
