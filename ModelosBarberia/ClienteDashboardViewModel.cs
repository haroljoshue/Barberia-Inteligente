namespace ModelosBarberia.ViewModels
{
    public class ClienteDashboardViewModel
    {
        public ApplicationUser Cliente { get; set; } = new();
        public List<Cita> HistorialCitas { get; set; } = new();
    }
}
