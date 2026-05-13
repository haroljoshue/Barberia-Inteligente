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
    public class LogSistemasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogSistemasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LogSistemas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogSistema>>> GetLogSistema()
        {
            return await _context.LogsSistema.ToListAsync();
        }

        // GET: api/LogSistemas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LogSistema>> GetLogSistema(int id)
        {
            var logSistema = await _context.LogsSistema.FindAsync(id);

            if (logSistema == null)
            {
                return NotFound();
            }

            return logSistema;
        }

        // PUT: api/LogSistemas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogSistema(int id, LogSistema logSistema)
        {
            if (id != logSistema.Id)
            {
                return BadRequest();
            }

            _context.Entry(logSistema).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogSistemaExists(id))
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

        // POST: api/LogSistemas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LogSistema>> PostLogSistema(LogSistema logSistema)
        {
            _context.LogsSistema.Add(logSistema);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLogSistema", new { id = logSistema.Id }, logSistema);
        }

        // DELETE: api/LogSistemas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogSistema(int id)
        {
            var logSistema = await _context.LogsSistema.FindAsync(id);
            if (logSistema == null)
            {
                return NotFound();
            }

            _context.LogsSistema.Remove(logSistema);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LogSistemaExists(int id)
        {
            return _context.LogsSistema.Any(e => e.Id == id);
        }
    }
}
