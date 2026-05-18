using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Barberia.MVC.Data;
using ModelosBarberia;
using ModelosBarberia.Enum;

namespace Barberia.MVC.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var viewModel = new AdminDashboardViewModel();

            try
            {
                // =========================
                // FECHAS - ECUADOR
                // =========================
                var ahoraEcuador = ObtenerFechaEcuador();
                var hoyInicio = ahoraEcuador.Date;
                var mananaInicio = hoyInicio.AddDays(1);

                // =========================
                // CITAS
                // =========================
                var citas = _context.Set<Cita>().AsQueryable();

                var totalCitas = citas.Count();

                viewModel.TotalCitas = totalCitas;

                viewModel.CitasHoy = citas.Count(c =>
                    c.FechaHora >= hoyInicio &&
                    c.FechaHora < mananaInicio
                );

                viewModel.CitasPendientes = citas.Count(c =>
                    c.Estado == EstadoCita.Pendiente
                );

                viewModel.CitasConVector = citas.Count(c =>
                    c.IdVector != null && c.IdVector != ""
                );

                viewModel.CitasSinVector = citas.Count(c =>
                    c.IdVector == null || c.IdVector == ""
                );

                viewModel.PorcentajeVectorizacion = totalCitas == 0
                    ? 0
                    : Math.Round((double)viewModel.CitasConVector / totalCitas * 100, 2);

                viewModel.CitasPorCliente = citas
                    .GroupBy(c => c.ClienteId)
                    .Select(g => new
                    {
                        Cliente = g.Key == null || g.Key == "" ? "Sin cliente" : g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Cliente, x => x.Cantidad);

                // =========================
                // USUARIOS
                // =========================
                viewModel.TotalUsuarios = _context.Users.Count();

                viewModel.UsuariosPorRol = _context.Users
                    .GroupBy(u => u.RolSistema)
                    .Select(g => new
                    {
                        Rol = g.Key == null || g.Key == "" ? "Sin Rol" : g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Rol, x => x.Cantidad);

                // =========================
                // BARBEROS
                // =========================
                viewModel.TotalBarberos = _context.Set<Barbero>()
                    .Count(b => b.Disponible);

                // =========================
                // SERVICIOS
                // =========================
                viewModel.TotalServicios = _context.Set<Servicio>()
                    .Count(s => s.Activo);

                /*
                 Esta versión NO depende de s.Citas.Count().
                 Cuenta las citas desde la tabla Citas y las une con Servicios.
                 Así es más confiable para el dashboard.
                */
                var reservasPorServicio = _context.Set<Cita>()
                    .GroupBy(c => c.ServicioId)
                    .Select(g => new
                    {
                        ServicioId = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList();

                var servicios = _context.Set<Servicio>()
                    .Select(s => new
                    {
                        s.Id,
                        s.Nombre
                    })
                    .ToList();

                viewModel.ReservasPorServicio = servicios
                    .GroupJoin(
                        reservasPorServicio,
                        servicio => servicio.Id,
                        reserva => reserva.ServicioId,
                        (servicio, reservas) => new
                        {
                            Nombre = servicio.Nombre,
                            Cantidad = reservas.Sum(r => r.Cantidad)
                        }
                    )
                    .ToDictionary(x => x.Nombre, x => x.Cantidad);

                // =========================
                // LOGS DEL AGENTE
                // =========================
                var logsAgente = _context.Set<LogAgente>()
                    .OrderByDescending(l => l.Fecha)
                    .ToList();

                viewModel.TotalConsultasAgente = logsAgente.Count;

                viewModel.ConsultasExitosasAgente = logsAgente.Count(l =>
                    l.ConsultaExitosa
                );

                viewModel.ConsultasErrorAgente = logsAgente.Count(l =>
                    !l.ConsultaExitosa ||
                    !string.IsNullOrWhiteSpace(l.MensajeError)
                );

                /*
                 Aquí ignoramos null y 0.
                 Esto es lo que pediste: no tomar en cuenta los datos
                 que todavía estás guardando en cero.
                */
                viewModel.TiempoPromedioRespuestaAgente = PromedioSeguro(
                    logsAgente
                        .Where(l => l.TiempoRespuestaMs.HasValue && l.TiempoRespuestaMs.Value > 0)
                        .Select(l => (double)l.TiempoRespuestaMs!.Value)
                );

                viewModel.TokensPromedioAgente = PromedioSeguro(
                    logsAgente
                        .Where(l => l.TokensUsados.HasValue && l.TokensUsados.Value > 0)
                        .Select(l => (double)l.TokensUsados!.Value)
                );

                viewModel.SimilitudPromedioAgente = PromedioSeguro(
                    logsAgente
                        .Where(l => l.SimilitudPromedio.HasValue && l.SimilitudPromedio.Value > 0)
                        .Select(l => (double)l.SimilitudPromedio!.Value),
                    4
                );

                viewModel.SimilitudMaximaAgente = MaximoSeguro(
                    logsAgente
                        .Where(l => l.SimilitudMaxima.HasValue && l.SimilitudMaxima.Value > 0)
                        .Select(l => (double)l.SimilitudMaxima!.Value),
                    4
                );

                /*
                 OJO:
                 Aquí sí contamos CantidadResultados == 0 porque eso significa
                 "consultas sin resultados". No es un promedio contaminado.
                */
                viewModel.ConsultasSinResultados = logsAgente.Count(l =>
                    l.CantidadResultados.HasValue &&
                    l.CantidadResultados.Value == 0
                );

                viewModel.HerramientasUsadas = logsAgente
                    .Where(l => !string.IsNullOrWhiteSpace(l.HerramientaUsada))
                    .GroupBy(l => l.HerramientaUsada!)
                    .Select(g => new
                    {
                        Herramienta = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToDictionary(x => x.Herramienta, x => x.Cantidad);

                viewModel.ConsultasPorTipo = logsAgente
                    .GroupBy(l => l.Tipo)
                    .Select(g => new
                    {
                        Tipo = g.Key.ToString(),
                        Cantidad = g.Count()
                    })
                    .ToDictionary(x => x.Tipo, x => x.Cantidad);

                viewModel.UltimosLogsAgente = logsAgente
                    .Take(10)
                    .ToList();

                // =========================
                // LOGS DEL SISTEMA
                // =========================
                var logsSistema = _context.Set<LogSistema>()
                    .OrderByDescending(l => l.Fecha)
                    .ToList();

                viewModel.TotalLogsSistema = logsSistema.Count;

                viewModel.LogsSistemaExitosos = logsSistema.Count(l =>
                    l.Exitoso
                );

                viewModel.LogsSistemaError = logsSistema.Count(l =>
                    !l.Exitoso
                );

                viewModel.LatenciaPromedioSistema = PromedioSeguro(
                    logsSistema
                        .Where(l => l.LatenciaMs.HasValue && l.LatenciaMs.Value > 0)
                        .Select(l => (double)l.LatenciaMs!.Value)
                );

                viewModel.LogsSistemaPorTipo = logsSistema
                    .GroupBy(l => l.Tipo)
                    .Select(g => new
                    {
                        Tipo = g.Key.ToString(),
                        Cantidad = g.Count()
                    })
                    .ToDictionary(x => x.Tipo, x => x.Cantidad);

                viewModel.LogsSistemaPorEntidad = logsSistema
                    .Where(l => !string.IsNullOrWhiteSpace(l.Entidad))
                    .GroupBy(l => l.Entidad!)
                    .Select(g => new
                    {
                        Entidad = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToDictionary(x => x.Entidad, x => x.Cantidad);

                viewModel.UltimosLogsSistema = logsSistema
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general cargando el Dashboard de Administración.");
            }

            return View(viewModel);
        }

        private static double PromedioSeguro(IEnumerable<double> valores, int decimales = 2)
        {
            var lista = valores.ToList();

            if (!lista.Any())
            {
                return 0;
            }

            return Math.Round(lista.Average(), decimales);
        }

        private static double MaximoSeguro(IEnumerable<double> valores, int decimales = 2)
        {
            var lista = valores.ToList();

            if (!lista.Any())
            {
                return 0;
            }

            return Math.Round(lista.Max(), decimales);
        }

        private static DateTime ObtenerFechaEcuador()
        {
            try
            {
                // Railway/Linux
                var zonaEcuador = TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaEcuador);
            }
            catch
            {
                try
                {
                    // Windows/Visual Studio
                    var zonaEcuador = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaEcuador);
                }
                catch
                {
                    // Respaldo simple UTC-5
                    return DateTime.UtcNow.AddHours(-5);
                }
            }
        }

        public class AdminDashboardViewModel
        {
            public Dictionary<string, int> ReservasPorServicio { get; set; } = new();
            public Dictionary<string, int> UsuariosPorRol { get; set; } = new();
            public Dictionary<string, int> CitasPorCliente { get; set; } = new();

            public int TotalCitas { get; set; }
            public int CitasHoy { get; set; }
            public int CitasPendientes { get; set; }
            public int TotalUsuarios { get; set; }
            public int TotalBarberos { get; set; }
            public int TotalServicios { get; set; }

            public int CitasConVector { get; set; }
            public int CitasSinVector { get; set; }
            public double PorcentajeVectorizacion { get; set; }
            public int DimensionEmbedding { get; set; } = 1536;

            // Métricas del agente
            public int TotalConsultasAgente { get; set; }
            public int ConsultasExitosasAgente { get; set; }
            public int ConsultasErrorAgente { get; set; }
            public double TiempoPromedioRespuestaAgente { get; set; }
            public double TokensPromedioAgente { get; set; }
            public int ConsultasSinResultados { get; set; }
            public double SimilitudPromedioAgente { get; set; }
            public double SimilitudMaximaAgente { get; set; }

            public Dictionary<string, int> HerramientasUsadas { get; set; } = new();
            public Dictionary<string, int> ConsultasPorTipo { get; set; } = new();
            public List<LogAgente> UltimosLogsAgente { get; set; } = new();

            // Métricas de logs del sistema
            public int TotalLogsSistema { get; set; }
            public int LogsSistemaExitosos { get; set; }
            public int LogsSistemaError { get; set; }
            public double LatenciaPromedioSistema { get; set; }

            public Dictionary<string, int> LogsSistemaPorTipo { get; set; } = new();
            public Dictionary<string, int> LogsSistemaPorEntidad { get; set; } = new();
            public List<LogSistema> UltimosLogsSistema { get; set; } = new();

        }
    }
}