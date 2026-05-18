using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using Barberia.Api.Data;
using ModelosBarberia.DTO_s;

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

        // GET: api/Barberos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BarberoDto>>> GetBarbero()
        {
            try
            {
                // Mapeamos directamente a DTO para romper la referencia circular de las Citas
                var barberos = await _context.Barberos
                    .Select(b => new BarberoDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        Nombre = b.Nombre,
                        Especialidad = b.Especialidad,
                        Telefono = b.Telefono,
                        Email = b.Email,
                        Disponible = b.Disponible,
                        FechaRegistro = b.FechaRegistro
                    })
                    .ToListAsync();

                return Ok(barberos);
            }
            catch (Exception ex)
            {
                // Si algo falla, te devolverá el mensaje de error real en Swagger en vez de un 500 genérico
                return StatusCode(500, $"Error interno: {ex.Message} -> {ex.InnerException?.Message}");
            }
        }

        // GET: api/Barberos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BarberoDto>> GetBarbero(int id)
        {
            try
            {
                var barbero = await _context.Barberos
                    .Where(b => b.Id == id)
                    .Select(b => new BarberoDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        Nombre = b.Nombre,
                        Especialidad = b.Especialidad,
                        Telefono = b.Telefono,
                        Email = b.Email,
                        Disponible = b.Disponible,
                        FechaRegistro = b.FechaRegistro
                    })
                    .FirstOrDefaultAsync();

                if (barbero == null)
                {
                    return NotFound();
                }

                return Ok(barbero);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // PUT: api/Barberos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBarbero(int id, Barbero barbero)
        {
            if (id != barbero.Id)
            {
                return BadRequest();
            }

            _context.Entry(barbero).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BarberoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Barberos
        [HttpPost]
        public async Task<ActionResult<Barbero>> PostBarbero(Barbero barbero)
        {
            _context.Barberos.Add(barbero);
            await _context.SaveChangesAsync();

            // Nota: Para evitar problemas con el CreatedAtAction por el cambio del tipo de retorno en el GET, 
            // usamos la ruta de forma explícita.
            return CreatedAtAction(nameof(GetBarbero), new { id = barbero.Id }, barbero);
        }

        // DELETE: api/Barberos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBarbero(int id)
        {
            var barbero = await _context.Barberos.FindAsync(id);
            if (barbero == null)
            {
                return NotFound();
            }

            _context.Barberos.Remove(barbero);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BarberoExists(int id)
        {
            return _context.Barberos.Any(e => e.Id == id);
        }
    }
}