using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;

namespace Barberia.MVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Aquí defines tus tablas
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Barbero> Barberos { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<LogSistema> LogsSistema { get; set; }
        public DbSet<LogAgente> LogsAgente { get; set; }
    }
}
