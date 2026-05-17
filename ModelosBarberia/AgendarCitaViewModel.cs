using ModelosBarberia;

namespace ModelosBarberia.ViewModels
{
    public class AgendarCitaViewModel
    {
        public List<ApplicationUser> Barberos { get; set; } = new();
        public List<Servicio> Servicios { get; set; } = new();
    }
}