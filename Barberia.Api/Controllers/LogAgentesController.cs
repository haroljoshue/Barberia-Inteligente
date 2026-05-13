using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using Barberia.Api.Data;

namespace Barberia.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogAgentesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogAgentesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LogAgentes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogAgente>>> GetLogAgente()
        {
            return await _context.LogsAgente.ToListAsync();
        }

        // GET: api/LogAgentes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LogAgente>> GetLogAgente(int id)
        {
            var logAgente = await _context.LogsAgente.FindAsync(id);

            if (logAgente == null)
            {
                return NotFound();
            }

            return logAgente;
        }

        // PUT: api/LogAgentes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogAgente(int id, LogAgente logAgente)
        {
            if (id != logAgente.Id)
            {
                return BadRequest();
            }

            _context.Entry(logAgente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogAgenteExists(id))
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

        // POST: api/LogAgentes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LogAgente>> PostLogAgente(LogAgente logAgente)
        {
            _context.LogsAgente.Add(logAgente);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLogAgente", new { id = logAgente.Id }, logAgente);
        }

        // DELETE: api/LogAgentes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogAgente(int id)
        {
            var logAgente = await _context.LogsAgente.FindAsync(id);
            if (logAgente == null)
            {
                return NotFound();
            }

            _context.LogsAgente.Remove(logAgente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LogAgenteExists(int id)
        {
            return _context.LogsAgente.Any(e => e.Id == id);
        }
    }
}
