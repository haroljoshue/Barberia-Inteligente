// Representa a un usuario del sistema con autenticación y roles
namespace ModelosBarberia
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;

    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [StringLength(50)]
        public string? RolSistema { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
        public ICollection<LogAgente> LogsAgente { get; set; } = new List<LogAgente>();
        public ICollection<LogSistema> LogsSistema { get; set; } = new List<LogSistema>();
    }
}
