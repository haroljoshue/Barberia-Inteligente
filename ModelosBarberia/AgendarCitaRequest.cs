public class AgendarCitaRequest
{
    public string ClienteId { get; set; } = string.Empty;  
    public int BarberoId { get; set; }                      
    public int ServicioId { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Observacion { get; set; }
}