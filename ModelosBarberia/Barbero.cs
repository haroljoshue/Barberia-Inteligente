// Representa a un barbero con sus datos, especialidad y disponibilidad
namespace ModelosBarberia
{
    using System.ComponentModel.DataAnnotations;

    public class Barbero
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Especialidad { get; set; }

        [Phone, StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        public bool Disponible { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
