namespace ModelosBarberia.ViewModels
{
    public class ClienteDashboardViewModel
    {
        public ApplicationUser Cliente { get; set; } = null!;
        public List<Cita> HistorialCitas { get; set; } = new();
        public int CitasPendientes { get; set; }
        public int CitasCompletadas { get; set; }
        public Cita? ProximaCita { get; set; }
    }
}