namespace ModelosBarberia.ViewModels
{
    public class BarberoDashboardViewModel
    {
        public List<Cita> Citas { get; set; } = new();
        public List<ApplicationUser> TopClientes { get; set; } = new();
        public int CitasHoy { get; set; }
        public int CitasPendientes { get; set; }
        public int CitasCompletadas { get; set; }
        public int TotalCitas { get; set; }
        public int CitasCanceladas { get; set; }
        public decimal GananciasTotales { get; set; }
        public List<(string Mes, int Total, decimal Ganancias)> CitasPorMes { get; set; }

    }
}