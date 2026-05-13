// Representa una cita entre cliente, barbero y servicio
namespace ModelosBarberia
{
    using ModelosBarberia.Enum;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Cita
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ClienteId { get; set; } = string.Empty;

        [ForeignKey("ClienteId")]
        public ApplicationUser Cliente { get; set; } = null!;

        [Required]
        public int BarberoId { get; set; }

        [ForeignKey("BarberoId")]
        public Barbero Barbero { get; set; } = null!;

        [Required]
        public int ServicioId { get; set; }

        [ForeignKey("ServicioId")]
        public Servicio Servicio { get; set; } = null!;

        [Required, DataType(DataType.DateTime)]
        public DateTime FechaHora { get; set; }

        [Required]
        public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;

        [StringLength(500)]
        public string? Observacion { get; set; }

        public decimal? PrecioFinal { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
