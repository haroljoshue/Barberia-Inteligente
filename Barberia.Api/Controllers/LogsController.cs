using Barberia.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using ModelosBarberia;
using ModelosBarberia.DTO_s;
using ModelosBarberia.DTOs;
using ModelosBarberia.Enum;

namespace Barberia.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("agente")]
        public async Task<IActionResult> RegistrarLogAgente([FromBody] RegistrarLogAgenteDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UsuarioId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El UsuarioId es obligatorio."
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Pregunta))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "La pregunta es obligatoria."
                });
            }

            var usuarioExiste = await _context.Users
                .AnyAsync(u => u.Id == dto.UsuarioId);

            if (!usuarioExiste)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "El UsuarioId no existe en AspNetUsers."
                });
            }

            var log = new LogAgente
            {
                UsuarioId = dto.UsuarioId,
                Tipo = dto.ConsultaExitosa ? dto.Tipo : TipoLogAgente.Error,
                Pregunta = dto.Pregunta,
                Respuesta = dto.Respuesta,
                TokensUsados = dto.TokensUsados,
                TiempoRespuestaMs = dto.TiempoRespuestaMs,
                HerramientaUsada = dto.HerramientaUsada,
                CantidadResultados = dto.CantidadResultados,
                SimilitudPromedio = dto.SimilitudPromedio,
                SimilitudMaxima = dto.SimilitudMaxima,
                ConsultaExitosa = dto.ConsultaExitosa,
                MensajeError = dto.MensajeError,
                Fecha = DateTime.UtcNow
            };

            _context.Set<LogAgente>().Add(log);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Log del agente registrado correctamente.",
                data = new
                {
                    log.Id,
                    log.UsuarioId,
                    Tipo = log.Tipo.ToString(),
                    log.HerramientaUsada,
                    log.ConsultaExitosa,
                    log.Fecha
                }
            });
        }
    }
}