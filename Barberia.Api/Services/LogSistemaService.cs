using Barberia.Api.Data;
using ModelosBarberia;
using ModelosBarberia.Enum;

namespace Barberia.Api.Services
{
    public class LogSistemaService
    {
        private readonly AppDbContext _context;

        public LogSistemaService(AppDbContext context)
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
