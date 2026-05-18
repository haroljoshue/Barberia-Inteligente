using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Barberia.Api.Data;
using ModelosBarberia;
using ModelosBarberia.DTOs;
using ModelosBarberia.Enum;
using ModelosBarberia.DTO_s;
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

        // GET: api/Citas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cita>>> GetCitas()
        {
            // Proyección limpia a la clase Cita para evitar bucles infinitos en el JSON
            return await _context.Citas
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
                    Cliente = c.Cliente == null ? null! : new ApplicationUser { Id = c.Cliente.Id, NombreCompleto = c.Cliente.NombreCompleto, Email = c.Cliente.Email },
                    Barbero = c.Barbero == null ? null! : new Barbero { Id = c.Barbero.Id, Nombre = c.Barbero.Nombre },
                    Servicio = c.Servicio == null ? null! : new Servicio { Id = c.Servicio.Id, Nombre = c.Servicio.Nombre, Precio = c.Servicio.Precio }
                })
                .ToListAsync();
        }

        // GET: api/Citas/cliente/{clienteId} -> REQUERIDO POR MVC (ClienteController)
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<Cita>>> ObtenerPorCliente(string clienteId)
        {
            return await _context.Citas
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
                    Barbero = c.Barbero == null ? null! : new Barbero { Id = c.Barbero.Id, Nombre = c.Barbero.Nombre, Especialidad = c.Barbero.Especialidad },
                    Servicio = c.Servicio == null ? null! : new Servicio { Id = c.Servicio.Id, Nombre = c.Servicio.Nombre, Precio = c.Servicio.Precio }
                })
                .ToListAsync();
        }

        // GET: api/Citas/barbero/{barberoId} -> REQUERIDO POR MVC (BarberoController)
        [HttpGet("barbero/{barberoId}")]
        public async Task<ActionResult<IEnumerable<CitaBarberoDto>>> ObtenerPorBarbero(int barberoId)
        {
            return await _context.Citas
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
        }

        // POST: api/Citas -> REQUERIDO POR MVC (Usa el Request plano para Swagger)
        [HttpPost]
        public async Task<ActionResult<Cita>> PostCita(AgendarCitaRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Buscamos el precio del servicio para asignarlo automáticamente a la cita
            var servicio = await _context.Servicios.FindAsync(request.ServicioId);
            if (servicio == null) return BadRequest("El servicio seleccionado no existe.");

            var cita = new Cita
            {
                ClienteId = request.ClienteId,
                ServicioId = request.ServicioId,
                BarberoId = request.BarberoId,
                FechaHora = request.FechaHora,
                Observacion = request.Observacion,
                Estado = EstadoCita.Pendiente,
                PrecioFinal = servicio.Precio, // Se llena con el precio real del servicio
                FechaRegistro = DateTime.UtcNow
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            // Seteamos las propiedades complejas en null para evitar referencias cíclicas al responder
            cita.Cliente = null!;
            cita.Barbero = null!;
            cita.Servicio = null!;

            return CreatedAtAction(nameof(GetCitas), new { id = cita.Id }, cita);
        }

        // PUT: api/Citas/5/estado -> REQUERIDO POR MVC (Cancelar y cambiar flujos)
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoCita estado)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            cita.Estado = estado;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Citas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCita(int id, Cita cita)
        {
            if (id != cita.Id) return BadRequest();

            _context.Entry(cita).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Citas.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Citas/5
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