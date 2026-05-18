using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using Barberia.Api.Data;
using ModelosBarberia.DTOs;
using ModelosBarberia.Enum;

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
            return await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Barbero)
                .Include(c => c.Servicio)
                .ToListAsync();
        }

        // GET: api/citas/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Cita>>> ObtenerPorCliente(string clienteId)
        {
            var citas = await _context.Citas
                .Include(c => c.Barbero)
                .Include(c => c.Servicio)
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaHora)
                .ToListAsync();

            return Ok(citas);
        }

        // GET: api/citas/barbero/{barberoId}
        [HttpGet("barbero/{barberoId}")]
        public async Task<ActionResult<IEnumerable<CitaBarberoDto>>> ObtenerPorBarbero(int barberoId)
        {
            var citas = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .Where(c => c.BarberoId == barberoId)
                .OrderBy(c => c.FechaHora)
                .Select(c => new CitaBarberoDto
                {
                    Id = c.Id,
                    ClienteNombre = c.Cliente != null ? c.Cliente.NombreCompleto : "Sin cliente",
                    ServicioNombre = c.Servicio != null ? c.Servicio.Nombre : "Sin servicio",
                    FechaHora = c.FechaHora,
                    Estado = (int)c.Estado,
                    Observacion = c.Observacion,
                    PrecioFinal = c.PrecioFinal
                })
                .ToListAsync();

            return Ok(citas);
        }

        // POST: api/citas
        [HttpPost]
        public async Task<ActionResult<Cita>> PostCita(AgendarCitaRequest request)
        {
            var cita = new Cita
            {
                ClienteId = request.ClienteId,
                ServicioId = request.ServicioId,
                BarberoId = request.BarberoId,
                FechaHora = request.FechaHora,
                Observacion = request.Observacion,
                Estado = EstadoCita.Pendiente
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            return Ok(cita);
        }


        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoCita estado)
        {
            var cita = await _context.Citas.FindAsync(id);

            if (cita == null)
                return NotFound();

            cita.Estado = estado;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT completo (opcional)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCita(int id, Cita cita)
        {
            if (id != cita.Id)
                return BadRequest();

            _context.Entry(cita).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _context.Citas.FindAsync(id);

            if (cita == null)
                return NotFound();

            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}