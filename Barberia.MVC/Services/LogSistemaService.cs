using Barberia.MVC.Data;
using ModelosBarberia;
using ModelosBarberia.Enum;

namespace Barberia.MVC.Services
{
    public class LogSistemaService
    {
        private readonly ApplicationDbContext _context;

        public LogSistemaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAsync(
            TipoLogSistema tipo,
            string mensaje,
            string? entidad = null,
            string? entidadId = null,
            string? stackTrace = null,
            string? usuarioId = null,
            int? latenciaMs = null,
            bool exitoso = true)
        {
            var log = new LogSistema
            {
                Tipo = tipo,
                Mensaje = mensaje,
                Entidad = entidad,
                EntidadId = entidadId,
                StackTrace = stackTrace,
                UsuarioId = usuarioId,
                LatenciaMs = latenciaMs,
                Exitoso = exitoso,
                Fecha = DateTime.UtcNow
            };

            _context.Set<LogSistema>().Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
