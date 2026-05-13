public class AgendarCitaRequest
{
    public string ClienteId { get; set; } = string.Empty;
    public string BarberoId { get; set; } = string.Empty;
    public int ServicioId { get; set; }
    public DateTime Fecha { get; set; }
}