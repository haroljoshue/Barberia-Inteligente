using Barberia.Api.Data;
using Barberia.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using ModelosBarberia.DTO_s;
using ModelosBarberia.Enum;


namespace Barberia.Api.Controllers
{
    public class N8nController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmbeddingService _embeddingService;
        private readonly QdrantService _qdrantService;

        public N8nController(
                AppDbContext context,
                EmbeddingService embeddingService,
                QdrantService qdrantService)
        {
            _context = context;
            _embeddingService = embeddingService;
            _qdrantService = qdrantService;
        }

        [HttpPost("agendar-cita")]
        public async Task<IActionResult> AgendarCita([FromBody] AgendarCitaN8nDto request)
        {
            if (!EsSolicitudValida(request, out var mensajeValidacion))
            {
                return BadRequest(new
                {
                    success = false,
                    message = mensajeValidacion
                });
            }

            var fechaHoraUtc = request.FechaHora.ToUniversalTime();

            var existeCita = await _context.Citas.AnyAsync(c =>
                c.BarberoId == request.BarberoId &&
                c.FechaHora == fechaHoraUtc &&
                c.Estado != EstadoCita.Cancelada
            );

            if (existeCita)
            {
                return Conflict(new
                {
                    success = false,
                    message = "El barbero ya tiene una cita registrada en esa fecha y hora."
                });
            }

            var servicio = await _context.Servicios
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.ServicioId);

            if (servicio == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "El servicio seleccionado no existe."
                });
            }

            var cita = new Cita
            {
                ClienteId = request.ClienteId.Trim(),
                BarberoId = request.BarberoId,
                ServicioId = request.ServicioId,
                FechaHora = fechaHoraUtc,
                Estado = EstadoCita.Pendiente,
                Observacion = request.Observacion?.Trim(),
                PrecioFinal = servicio.Precio,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            string? idVector = null;

            try
            {
                idVector = Guid.NewGuid().ToString();

                var textoEmbedding = ConstruirTextoEmbedding(cita);

                var embedding = await _embeddingService.GenerarEmbeddingAsync(textoEmbedding);

                var payload = new
                {
                    idCita = cita.Id,
                    clienteId = cita.ClienteId,
                    barberoId = cita.BarberoId,
                    servicioId = cita.ServicioId,
                    fechaHora = cita.FechaHora,
                    estado = cita.Estado.ToString(),
                    observacion = cita.Observacion,
                    precioFinal = cita.PrecioFinal,
                    texto = textoEmbedding
                };

                await _qdrantService.GuardarVectorCitaAsync(idVector, embedding, payload);

                cita.IdVector = idVector;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = true,
                    warning = true,
                    message = "La cita fue agendada en la base relacional, pero no se pudo guardar el vector en Qdrant.",
                    error = ex.Message,
                    data = new
                    {
                        idCita = cita.Id,
                        clienteId = cita.ClienteId,
                        barberoId = cita.BarberoId,
                        servicioId = cita.ServicioId,
                        fechaHora = cita.FechaHora,
                        estado = cita.Estado.ToString(),
                        observacion = cita.Observacion,
                        precioFinal = cita.PrecioFinal,
                        fechaRegistro = cita.FechaRegistro,
                        idVector = cita.IdVector
                    }
                });
            }

            return Ok(new
            {
                success = true,
                message = "Cita agendada correctamente y vector guardado en Qdrant.",
                data = new
                {
                    idCita = cita.Id,
                    clienteId = cita.ClienteId,
                    barberoId = cita.BarberoId,
                    servicioId = cita.ServicioId,
                    fechaHora = cita.FechaHora,
                    estado = cita.Estado.ToString(),
                    observacion = cita.Observacion,
                    precioFinal = cita.PrecioFinal,
                    fechaRegistro = cita.FechaRegistro,
                    idVector = cita.IdVector
                }
            });
        }

        [HttpPost("buscar-similares")]
        public async Task<IActionResult> BuscarSimilares([FromBody] BuscarSimilarN8nDto request)
        {
            if (string.IsNullOrWhiteSpace(request.ClienteId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Debe enviar el identificador del cliente."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Consulta))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Debe enviar una descripción para buscar citas similares."
                });
            }

            try
            {
                var embeddingConsulta = await _embeddingService.GenerarEmbeddingAsync(request.Consulta);

                var resultados = await _qdrantService.BuscarSimilaresAsync(
                    embeddingConsulta,
                    request.ClienteId,
                    5
                );

                return Ok(new
                {
                    success = true,
                    message = "Búsqueda semántica realizada correctamente.",
                    consulta = request.Consulta,
                    total = resultados.Count,
                    data = resultados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "No se pudo realizar la búsqueda semántica.",
                    error = ex.Message
                });
            }
        }

        private static string ConstruirTextoEmbedding(Cita cita)
        {
            var observacion = string.IsNullOrWhiteSpace(cita.Observacion)
                ? "Sin observación"
                : cita.Observacion.Trim();

            return
                $"Cliente {cita.ClienteId} agendó una cita " +
                $"para el servicio {cita.ServicioId} " +
                $"con el barbero {cita.BarberoId} " +
                $"el día {cita.FechaHora:yyyy-MM-dd} " +
                $"a las {cita.FechaHora:HH:mm}. " +
                $"Estado de la cita: {cita.Estado}. " +
                $"Precio final: {cita.PrecioFinal}. " +
                $"Observación: {observacion}.";
        }

        [HttpGet("mis-citas/{clienteId}")]
        public async Task<IActionResult> MisCitas(string clienteId)
        {
            if (string.IsNullOrWhiteSpace(clienteId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Debe enviar el identificador del cliente."
                });
            }

            var citas = await _context.Citas
                .AsNoTracking()
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaHora)
                .Select(c => new
                {
                    idCita = c.Id,
                    barberoId = c.BarberoId,
                    servicioId = c.ServicioId,
                    fechaHora = c.FechaHora,
                    estado = c.Estado.ToString(),
                    observacion = c.Observacion,
                    precioFinal = c.PrecioFinal
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                total = citas.Count,
                data = citas
            });
        }

        [HttpGet("proximas-citas/{clienteId}")]
        public async Task<IActionResult> ProximasCitas(string clienteId)
        {
            if (string.IsNullOrWhiteSpace(clienteId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Debe enviar el identificador del cliente."
                });
            }

            var ahora = DateTime.UtcNow;

            var citas = await _context.Citas
                .AsNoTracking()
                .Where(c =>
                    c.ClienteId == clienteId &&
                    c.FechaHora >= ahora &&
                    c.Estado != EstadoCita.Cancelada)
                .OrderBy(c => c.FechaHora)
                .Select(c => new
                {
                    idCita = c.Id,
                    barberoId = c.BarberoId,
                    servicioId = c.ServicioId,
                    fechaHora = c.FechaHora,
                    estado = c.Estado.ToString(),
                    observacion = c.Observacion,
                    precioFinal = c.PrecioFinal
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                total = citas.Count,
                data = citas
            });
        }

        private static bool EsSolicitudValida(AgendarCitaN8nDto request, out string mensaje)
        {
            if (string.IsNullOrWhiteSpace(request.ClienteId))
            {
                mensaje = "Debe enviar el identificador del cliente.";
                return false;
            }

            if (request.BarberoId <= 0)
            {
                mensaje = "Debe enviar un barbero válido.";
                return false;
            }

            if (request.ServicioId <= 0)
            {
                mensaje = "Debe enviar un servicio válido.";
                return false;
            }

            if (request.FechaHora == default)
            {
                mensaje = "Debe enviar la fecha y hora de la cita.";
                return false;
            }

            mensaje = string.Empty;
            return true;
        }

    }
}
