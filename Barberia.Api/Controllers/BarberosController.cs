using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using Barberia.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelosBarberia.Enum;

namespace Barberia.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarberosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BarberosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/barberos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Barbero>>> GetBarberos()
        {
            try
            {
                // Seleccionamos las columnas de forma explícita mapeando a la entidad original.
                // Esto ignora cualquier columna fantasma (como UserId) y rompe la recursión de Citas.
                var barberos = await _context.Barberos
                    .Select(b => new Barbero
                    {
                        Id = b.Id,
                        Nombre = b.Nombre,
                        Especialidad = b.Especialidad,
                        Telefono = b.Telefono,
                        Email = b.Email,
                        Disponible = b.Disponible,
                        FechaRegistro = b.FechaRegistro,
                        Citas = new List<Cita>() // Se envía vacío para evitar bucles en el JSON
                    })
                    .ToListAsync();

                return Ok(barberos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener los barberos", detalle = ex.Message });
            }
        }

        // GET: api/barberos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Barbero>> GetBarbero(int id)
        {
            try
            {
                var barbero = await _context.Barberos
                    .Where(b => b.Id == id)
                    .Select(b => new Barbero
                    {
                        Id = b.Id,
                        Nombre = b.Nombre,
                        Especialidad = b.Especialidad,
                        Telefono = b.Telefono,
                        Email = b.Email,
                        Disponible = b.Disponible,
                        FechaRegistro = b.FechaRegistro,
                        Citas = new List<Cita>()
                    })
                    .FirstOrDefaultAsync();

                if (barbero == null)
                    return NotFound(new { mensaje = "Barbero no encontrado" });

                return Ok(barbero);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener el barbero", detalle = ex.Message });
            }
        }

        // POST: api/barberos
        [HttpPost]
        public async Task<ActionResult<Barbero>> PostBarbero([FromBody] Barbero barbero)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Aseguramos valores por defecto si es necesario antes de guardar
                barbero.FechaRegistro = DateTime.UtcNow;
                barbero.Citas = null!; // Evitamos que intente insertar datos basura en cascada

                _context.Barberos.Add(barbero);
                await _context.SaveChangesAsync();

                // Limpiamos la navegación para la respuesta JSON limpia
                barbero.Citas = new List<Cita>();

                return CreatedAtAction(nameof(GetBarbero), new { id = barbero.Id }, barbero);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al guardar el barbero", detalle = ex.Message });
            }
        }

        // PUT: api/barberos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBarbero(int id, [FromBody] Barbero barbero)
        {
            if (id != barbero.Id) return BadRequest(new { mensaje = "El ID no coincide con el cuerpo de la solicitud" });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Buscamos el registro existente para actualizar únicamente las columnas necesarias
            var registroExistente = await _context.Barberos.FindAsync(id);
            if (registroExistente == null) return NotFound(new { mensaje = "Barbero no encontrado" });

            try
            {
                // Actualizamos las propiedades individualmente, previniendo errores de rastreo de entidades
                registroExistente.Nombre = barbero.Nombre;
                registroExistente.Especialidad = barbero.Especialidad;
                registroExistente.Telefono = barbero.Telefono;
                registroExistente.Email = barbero.Email;
                registroExistente.Disponible = barbero.Disponible;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Barberos.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar el barbero", detalle = ex.Message });
            }
        }

        // DELETE: api/barberos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBarbero(int id)
        {
            try
            {
                var barbero = await _context.Barberos.FindAsync(id);
                if (barbero == null) return NotFound(new { mensaje = "Barbero no encontrado" });

                _context.Barberos.Remove(barbero);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Barbero eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar el barbero. Verifique si tiene citas asociadas.", detalle = ex.Message });
            }
        }

        // GET: api/Citas/barbero/{barberoId}/dashboard
        [HttpGet("barbero/{barberoId}/dashboard")]
        public async Task<ActionResult<object>> ObtenerDashboardBarbero(int barberoId)
        {
            var citas = await _context.Citas
                .Where(c => c.BarberoId == barberoId)
                .ToListAsync();

            var totalCitas = citas.Count;
            var citasPendientes = citas.Count(c => c.Estado == EstadoCita.Pendiente);
            var citasCompletadas = citas.Count(c => c.Estado == EstadoCita.Atendida);
            var citasCanceladas = citas.Count(c => c.Estado == EstadoCita.Cancelada);

            var gananciasTotales = citas
                .Where(c => c.Estado == EstadoCita.Atendida)
                .Sum(c => c.PrecioFinal);

            var citasPorMes = citas
                .GroupBy(c => new { c.FechaHora.Year, c.FechaHora.Month })
                .Select(g => new {
                    Mes = $"{g.Key.Month}/{g.Key.Year}",
                    Total = g.Count(),
                    Ganancias = g.Where(c => c.Estado == EstadoCita.Atendida).Sum(c => c.PrecioFinal)
                });

            return Ok(new
            {
                TotalCitas = totalCitas,
                Pendientes = citasPendientes,
                Completadas = citasCompletadas,
                Canceladas = citasCanceladas,
                GananciasTotales = gananciasTotales,
                CitasPorMes = citasPorMes
            });
        }

    }
}