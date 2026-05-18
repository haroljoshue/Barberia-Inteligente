using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using Barberia.Api.Data;
using ModelosBarberia.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Barberia.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CitasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/citas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cita>>> GetCitas()
        {
            // Construimos estructuras limpias mapeadas a la entidad original para que MVC la entienda
            var citas = await _context.Citas
                .Select(c => new Cita
                {
                    Id = c.Id,
                    ClienteId = c.ClienteId,
                    BarberoId = c.BarberoId,
                    ServicioId = c.ServicioId,
                    FechaHora = c.FechaHora,
                    Estado = c.Estado,
                    Observacion = c.Observacion,
                    PrecioFinal = c.PrecioFinal,
                    FechaRegistro = c.FechaRegistro,
                    IdVector = c.IdVector,
                    Cliente = c.Cliente == null ? null! : new ApplicationUser
                    {
                        Id = c.Cliente.Id,
                        NombreCompleto = c.Cliente.NombreCompleto,
                        Email = c.Cliente.Email
                    },
                    Barbero = c.Barbero == null ? null! : new Barbero
                    {
                        Id = c.Barbero.Id,
                        Nombre = c.Barbero.Nombre
                    },
                    Servicio = c.Servicio == null ? null! : new Servicio
                    {
                        Id = c.Servicio.Id,
                        Nombre = c.Servicio.Nombre,
                        Precio = c.Servicio.Precio
                    }
                })
                .ToListAsync();

            return Ok(citas);
        }

        // GET: api/citas/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Cita>>> ObtenerPorCliente(string clienteId)
        {
            // Este endpoint lo lee directamente tu index del MVC como List<Cita>
            var citas = await _context.Citas
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaHora)
                .Select(c => new Cita
                {
                    Id = c.Id,
                    ClienteId = c.ClienteId,
                    BarberoId = c.BarberoId,
                    ServicioId = c.ServicioId,
                    FechaHora = c.FechaHora,
                    Estado = c.Estado,
                    Observacion = c.Observacion,
                    PrecioFinal = c.PrecioFinal,
                    FechaRegistro = c.FechaRegistro,
                    Barbero = c.Barbero == null ? null! : new Barbero
                    {
                        Id = c.Barbero.Id,
                        Nombre = c.Barbero.Nombre,
                        Especialidad = c.Barbero.Especialidad
                    },
                    Servicio = c.Servicio == null ? null! : new Servicio
                    {
                        Id = c.Servicio.Id,
                        Nombre = c.Servicio.Nombre,
                        Precio = c.Servicio.Precio
                    }
                })
                .ToListAsync();

            return Ok(citas);
        }

        // POST: api/citas
        [HttpPost]
        public async Task<ActionResult<Cita>> PostCita(AgendarCitaRequest request)
        {
            try
            {
                var cita = new Cita
                {
                    ClienteId = request.ClienteId,
                    ServicioId = request.ServicioId,
                    BarberoId = request.BarberoId,
                    FechaHora = request.FechaHora,
                    Observacion = request.Observacion,
                    Estado = EstadoCita.Pendiente,
                    FechaRegistro = DateTime.UtcNow
                };

                _context.Citas.Add(cita);
                await _context.SaveChangesAsync();

                // Para que no de error 500 al serializar la respuesta, instanciamos propiedades de navegación vacías
                cita.Cliente = null!;
                cita.Barbero = null!;
                cita.Servicio = null!;

                return Ok(cita);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al guardar: {ex.Message}");
            }
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoCita estado)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            cita.Estado = estado;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCita(int id, Cita cita)
        {
            if (id != cita.Id) return BadRequest();

            _context.Entry(cita).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}