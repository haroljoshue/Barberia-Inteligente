// Registra eventos del sistema como errores, advertencias y operaciones
namespace ModelosBarberia
{
    using ModelosBarberia.Enum;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class LogSistema
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public TipoLogSistema Tipo { get; set; }

        [Required, StringLength(200)]
        public string Mensaje { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Entidad { get; set; }

        [StringLength(50)]
        public string? EntidadId { get; set; }

        public string? StackTrace { get; set; }
        public string? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public ApplicationUser? Usuario { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
