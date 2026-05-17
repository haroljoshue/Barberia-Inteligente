namespace ModelosBarberia.DTOs
{
    public class CitaBarberoDto
    {
        public int Id { get; set; }

        public string ClienteNombre { get; set; } = string.Empty;

        public string ServicioNombre { get; set; } = string.Empty;

        public DateTime FechaHora { get; set; }

        public int Estado { get; set; }

        public string? Observacion { get; set; }

        public decimal? PrecioFinal { get; set; }
    }
}