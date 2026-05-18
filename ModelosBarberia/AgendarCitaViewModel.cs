namespace ModelosBarberia.ViewModels
{
    public class AgendarCitaViewModel
    {
        public List<BarberoSelectItem> Barberos { get; set; } = new();
        public List<Servicio> Servicios { get; set; } = new();
    }

    public class BarberoSelectItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}