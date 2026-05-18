using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelosBarberia.DTO_s
{
    public class BarberoDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Nombre { get; set; }
        public string Especialidad { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool Disponible { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
