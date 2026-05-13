namespace ModelosBarberia.ViewModels
{
    public class BarberoDashboardViewModel
    {
        public List<Cita> Citas { get; set; } = new();
        public List<ApplicationUser> TopClientes { get; set; } = new();
    }
}
