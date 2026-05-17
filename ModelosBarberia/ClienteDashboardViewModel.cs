namespace ModelosBarberia.ViewModels
{
    public class ClienteDashboardViewModel
    {
        public ApplicationUser Cliente { get; set; }
        public List<Cita> HistorialCitas { get; set; } = new();
        public int CitasPendientes { get; set; }
        public int CitasCompletadas { get; set; }
        public Cita? ProximaCita { get; set; }

        // ¡ASEGÚRATE DE QUE ESTA LÍNEA EXISTA AQUÍ!
        public List<Servicio> Servicios { get; set; } = new();
    }
}