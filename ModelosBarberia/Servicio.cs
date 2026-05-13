// Representa un servicio de la barbería con precio, duración y estado
namespace ModelosBarberia
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Servicio
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Descripcion { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        [Required]
        public int DuracionMinutos { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
