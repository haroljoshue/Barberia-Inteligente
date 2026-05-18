using ModelosBarberia.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelosBarberia.DTO_s
{
    public class RegistrarLogAgenteDto
    {
        public string UsuarioId { get; set; } = string.Empty;
        public TipoLogAgente Tipo { get; set; }

        public string Pregunta { get; set; } = string.Empty;
        public string? Respuesta { get; set; }

        public int? TokensUsados { get; set; }
        public int? TiempoRespuestaMs { get; set; }

        public string? HerramientaUsada { get; set; }

        public int? CantidadResultados { get; set; }

        public decimal? SimilitudPromedio { get; set; }
        public decimal? SimilitudMaxima { get; set; }

        public bool ConsultaExitosa { get; set; } = true;

        public string? MensajeError { get; set; }
    }
}
