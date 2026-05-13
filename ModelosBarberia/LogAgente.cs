// Registra interacciones con el agente de IA
namespace ModelosBarberia
{
    using ModelosBarberia.Enum;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class LogAgente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public ApplicationUser Usuario { get; set; } = null!;

        [Required]
        public TipoLogAgente Tipo { get; set; }

        [Required, StringLength(1000)]
        public string Pregunta { get; set; } = string.Empty;

        public string? Respuesta { get; set; }
        public int? TokensUsados { get; set; }
        public int? TiempoRespuestaMs { get; set; }

        [StringLength(500)]
        public string? MensajeError { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
