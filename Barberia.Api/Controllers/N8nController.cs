using Barberia.Api.Data;
using Barberia.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using ModelosBarberia.DTO_s;
using ModelosBarberia.Enum;
using Npgsql;
using System.Diagnostics;
using Barberia.Api.Services;


namespace Barberia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class N8nController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmbeddingService _embeddingService;
        private readonly QdrantService _qdrantService;
        private readonly LogSistemaService _logSistemaService;

        public N8nController(
        AppDbContext context,
        EmbeddingService embeddingService,
        QdrantService qdrantService,
        LogSistemaService logSistemaService)
        {
            _context = context;
            _embeddingService = embeddingService;
            _qdrantService = qdrantService;
            _logSistemaService = logSistemaService;
        }

        [HttpPost("agendar-cita")]
        public async Task<IActionResult> AgendarCita([FromBody] AgendarCitaN8nDto request)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (!EsSolicitudValida(request, out var mensajeValidacion))
                {
                    stopwatch.Stop();

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Advertencia,
                        mensaje: mensajeValidacion,
                        entidad: "Cita",
                        entidadId: null,
                        stackTrace: null,
                        usuarioId: request.ClienteId,
                        latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                        exitoso: false
                    );

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
                    stopwatch.Stop();

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Advertencia,
                        mensaje: "Intento de agendar una cita en un horario ocupado.",
                        entidad: "Cita",
                        entidadId: null,
                        stackTrace: null,
                        usuarioId: request.ClienteId,
                        latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                        exitoso: false
                    );

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
                    stopwatch.Stop();

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Error,
                        mensaje: "El servicio seleccionado no existe.",
                        entidad: "Servicio",
                        entidadId: request.ServicioId.ToString(),
                        stackTrace: null,
                        usuarioId: request.ClienteId,
                        latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                        exitoso: false
                    );

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
                    stopwatch.Stop();

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Advertencia,
                        mensaje: "La cita fue agendada, pero no se pudo guardar el vector en Qdrant.",
                        entidad: "Cita",
                        entidadId: cita.Id.ToString(),
                        stackTrace: ex.ToString(),
                        usuarioId: cita.ClienteId,
                        latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                        exitoso: true
                    );

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

                stopwatch.Stop();

                await _logSistemaService.RegistrarAsync(
                    tipo: TipoLogSistema.Insercion,
                    mensaje: "Cita agendada correctamente y vector guardado en Qdrant.",
                    entidad: "Cita",
                    entidadId: cita.Id.ToString(),
                    stackTrace: null,
                    usuarioId: cita.ClienteId,
                    latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                    exitoso: true
                );

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
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _logSistemaService.RegistrarAsync(
                    tipo: TipoLogSistema.Error,
                    mensaje: "Error general al agendar cita.",
                    entidad: "Cita",
                    entidadId: null,
                    stackTrace: ex.ToString(),
                    usuarioId: request.ClienteId,
                    latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                    exitoso: false
                );

                return StatusCode(500, new
                {
                    success = false,
                    message = "No se pudo agendar la cita.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("buscar-similares")]
        public async Task<IActionResult> BuscarSimilares([FromBody] BuscarSimilarN8nDto request)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (string.IsNullOrWhiteSpace(request.ClienteId))
                {
                    stopwatch.Stop();

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Advertencia,
                        mensaje: "Búsqueda semántica rechazada: clienteId vacío.",
                        entidad: "Cita",
                        entidadId: null,
                        stackTrace: null,
                        usuarioId: null,
                        latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                        exitoso: false
                    );

                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe enviar el identificador del cliente."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Consulta))
                {
                    stopwatch.Stop();

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Advertencia,
                        mensaje: "Búsqueda semántica rechazada: consulta vacía.",
                        entidad: "Cita",
                        entidadId: null,
                        stackTrace: null,
                        usuarioId: request.ClienteId,
                        latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                        exitoso: false
                    );

                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe enviar una descripción para buscar citas similares."
                    });
                }

                var embeddingConsulta = await _embeddingService.GenerarEmbeddingAsync(request.Consulta);

                var resultados = await _qdrantService.BuscarSimilaresAsync(
                    embeddingConsulta,
                    request.ClienteId,
                    5
                );

                stopwatch.Stop();

                await _logSistemaService.RegistrarAsync(
                    tipo: TipoLogSistema.ConsultaSemantica,
                    mensaje: $"Búsqueda semántica realizada correctamente. Resultados: {resultados.Count}.",
                    entidad: "Cita",
                    entidadId: null,
                    stackTrace: null,
                    usuarioId: request.ClienteId,
                    latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                    exitoso: true
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
                stopwatch.Stop();

                await _logSistemaService.RegistrarAsync(
                    tipo: TipoLogSistema.Error,
                    mensaje: "Error en búsqueda semántica.",
                    entidad: "Cita",
                    entidadId: null,
                    stackTrace: ex.ToString(),
                    usuarioId: request.ClienteId,
                    latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                    exitoso: false
                );

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

        [HttpPost("verificar-disponibilidad")]
        public async Task<IActionResult> VerificarDisponibilidad([FromBody] VerificarDisponibilidadDto dto)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (dto.BarberoId <= 0)
                {
                    stopwatch.Stop();

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Advertencia,
                        mensaje: "Verificación rechazada: barbero inválido.",
                        entidad: "Cita",
                        entidadId: null,
                        stackTrace: null,
                        usuarioId: null,
                        latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                        exitoso: false
                    );

                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe enviar un barbero válido."
                    });
                }

                if (dto.FechaHora == default)
                {
                    stopwatch.Stop();

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Advertencia,
                        mensaje: "Verificación rechazada: fecha y hora vacía.",
                        entidad: "Cita",
                        entidadId: null,
                        stackTrace: null,
                        usuarioId: null,
                        latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                        exitoso: false
                    );

                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe enviar la fecha y hora a verificar."
                    });
                }

                var fechaHoraUtc = dto.FechaHora.ToUniversalTime();

                var existeCita = await _context.Citas.AnyAsync(c =>
                    c.BarberoId == dto.BarberoId &&
                    c.FechaHora == fechaHoraUtc &&
                    c.Estado != EstadoCita.Cancelada
                );

                var disponible = !existeCita;

                stopwatch.Stop();

                await _logSistemaService.RegistrarAsync(
                    tipo: TipoLogSistema.Operacion,
                    mensaje: disponible
                        ? "Horario disponible verificado correctamente."
                        : "Horario ocupado detectado durante la verificación.",
                    entidad: "Cita",
                    entidadId: null,
                    stackTrace: null,
                    usuarioId: null,
                    latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                    exitoso: true
                );

                return Ok(new
                {
                    success = true,
                    disponible,
                    message = disponible
                        ? "El horario está disponible."
                        : "El barbero ya tiene una cita en ese horario.",
                    data = new
                    {
                        barberoId = dto.BarberoId,
                        fechaHora = fechaHoraUtc
                    }
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _logSistemaService.RegistrarAsync(
                    tipo: TipoLogSistema.Error,
                    mensaje: "Error al verificar disponibilidad.",
                    entidad: "Cita",
                    entidadId: null,
                    stackTrace: ex.ToString(),
                    usuarioId: null,
                    latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                    exitoso: false
                );

                return StatusCode(500, new
                {
                    success = false,
                    message = "No se pudo verificar disponibilidad.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("horarios-disponibles")]
        public async Task<IActionResult> HorariosDisponibles([FromBody] HorariosDisponiblesDto dto)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (dto.BarberoId <= 0)
                {
                    stopwatch.Stop();

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Advertencia,
                        mensaje: "Consulta de horarios rechazada: barbero inválido.",
                        entidad: "Cita",
                        entidadId: null,
                        stackTrace: null,
                        usuarioId: null,
                        latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                        exitoso: false
                    );

                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe enviar un barbero válido."
                    });
                }

                if (dto.Fecha == default)
                {
                    stopwatch.Stop();

                    await _logSistemaService.RegistrarAsync(
                        tipo: TipoLogSistema.Advertencia,
                        mensaje: "Consulta de horarios rechazada: fecha inválida.",
                        entidad: "Cita",
                        entidadId: null,
                        stackTrace: null,
                        usuarioId: null,
                        latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                        exitoso: false
                    );

                    return BadRequest(new
                    {
                        success = false,
                        message = "Debe enviar una fecha válida."
                    });
                }

                var fecha = dto.Fecha.Date;

                var inicioDia = DateTime.SpecifyKind(fecha, DateTimeKind.Local).ToUniversalTime();
                var finDia = DateTime.SpecifyKind(fecha.AddDays(1), DateTimeKind.Local).ToUniversalTime();

                var citasOcupadas = await _context.Citas
                    .AsNoTracking()
                    .Where(c =>
                        c.BarberoId == dto.BarberoId &&
                        c.FechaHora >= inicioDia &&
                        c.FechaHora < finDia &&
                        c.Estado != EstadoCita.Cancelada)
                    .Select(c => c.FechaHora)
                    .ToListAsync();

                var horariosBase = new List<TimeSpan>
        {
            new TimeSpan(9, 0, 0),
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            new TimeSpan(12, 0, 0),
            new TimeSpan(14, 0, 0),
            new TimeSpan(15, 0, 0),
            new TimeSpan(16, 0, 0),
            new TimeSpan(17, 0, 0),
            new TimeSpan(18, 0, 0),
            new TimeSpan(19, 0, 0),
            new TimeSpan(20, 0, 0),
            new TimeSpan(21, 0, 0)
        };

                var horariosDisponibles = horariosBase
                    .Select(hora => fecha.Add(hora))
                    .Where(horario =>
                    {
                        var horarioUtc = DateTime.SpecifyKind(horario, DateTimeKind.Local).ToUniversalTime();

                        return !citasOcupadas.Any(c =>
                            c.Year == horarioUtc.Year &&
                            c.Month == horarioUtc.Month &&
                            c.Day == horarioUtc.Day &&
                            c.Hour == horarioUtc.Hour &&
                            c.Minute == horarioUtc.Minute);
                    })
                    .Select(horario => new
                    {
                        fechaHora = horario.ToString("yyyy-MM-ddTHH:mm:ss"),
                        hora = horario.ToString("HH:mm")
                    })
                    .ToList();

                stopwatch.Stop();

                await _logSistemaService.RegistrarAsync(
                    tipo: TipoLogSistema.Operacion,
                    mensaje: $"Horarios disponibles consultados correctamente. Disponibles: {horariosDisponibles.Count}.",
                    entidad: "Cita",
                    entidadId: null,
                    stackTrace: null,
                    usuarioId: null,
                    latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                    exitoso: true
                );

                return Ok(new
                {
                    success = true,
                    message = "Horarios disponibles consultados correctamente.",
                    data = new
                    {
                        barberoId = dto.BarberoId,
                        fecha = fecha.ToString("yyyy-MM-dd"),
                        totalDisponibles = horariosDisponibles.Count,
                        horariosDisponibles
                    }
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _logSistemaService.RegistrarAsync(
                    tipo: TipoLogSistema.Error,
                    mensaje: "Error al consultar horarios disponibles.",
                    entidad: "Cita",
                    entidadId: null,
                    stackTrace: ex.ToString(),
                    usuarioId: null,
                    latenciaMs: (int)stopwatch.ElapsedMilliseconds,
                    exitoso: false
                );

                return StatusCode(500, new
                {
                    success = false,
                    message = "No se pudo consultar horarios disponibles.",
                    error = ex.Message
                });
            }
        }

    }
}
