using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelosBarberia.DTO_s
{
    public class AgendarCitaN8nDto
    {
        public string ClienteId { get; set; } = string.Empty;
        public int BarberoId { get; set; }
        public int ServicioId { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Observacion { get; set; }
    }
}
