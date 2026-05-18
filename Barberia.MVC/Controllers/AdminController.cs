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
                var totalCitas = _context.Set<Cita>().Count();

                var citasConVector = _context.Set<Cita>()
                    .Count(c => c.IdVector != null && c.IdVector != "");

                var citasSinVector = _context.Set<Cita>()
                    .Count(c => c.IdVector == null || c.IdVector == "");

                var hoyInicio = DateTime.UtcNow.Date;
                var mananaInicio = hoyInicio.AddDays(1);

                viewModel.TotalCitas = totalCitas;

                viewModel.CitasHoy = _context.Set<Cita>()
                    .Count(c => c.FechaHora >= hoyInicio && c.FechaHora < mananaInicio);

                viewModel.CitasPendientes = _context.Set<Cita>()
                    .Count(c => c.Estado == EstadoCita.Pendiente);

                viewModel.CitasConVector = citasConVector;
                viewModel.CitasSinVector = citasSinVector;
                viewModel.PorcentajeVectorizacion = totalCitas == 0
                    ? 0
                    : Math.Round((double)citasConVector / totalCitas * 100, 2);

                viewModel.CitasPorCliente = _context.Set<Cita>()
                    .GroupBy(c => c.ClienteId)
                    .Select(g => new
                    {
                        Cliente = g.Key ?? "Sin cliente",
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Cliente, x => x.Cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando métricas de citas en Admin.");
            }

            try
            {
                viewModel.TotalUsuarios = _context.Users.Count();

                viewModel.UsuariosPorRol = _context.Users
                    .GroupBy(u => u.RolSistema)
                    .Select(g => new
                    {
                        Rol = g.Key ?? "Sin Rol",
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Rol, x => x.Cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando métricas de usuarios en Admin.");
            }

            try
            {
                viewModel.TotalBarberos = _context.Set<Barbero>()
                    .Count(b => b.Disponible);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando métricas de barberos en Admin.");
            }

            try
            {
                viewModel.TotalServicios = _context.Set<Servicio>()
                    .Count(s => s.Activo);

                viewModel.ReservasPorServicio = _context.Set<Servicio>()
                    .Select(s => new
                    {
                        Nombre = s.Nombre,
                        Cantidad = s.Citas.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Nombre, x => x.Cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando métricas de servicios en Admin.");
            }

            try
            {
                var logsAgente = _context.Set<LogAgente>();

                viewModel.TotalConsultasAgente = logsAgente.Count();

                viewModel.ConsultasExitosasAgente = logsAgente
                    .Count(l => l.ConsultaExitosa);

                viewModel.ConsultasErrorAgente = logsAgente
                    .Count(l => !l.ConsultaExitosa || l.MensajeError != null);

                var tiempoPromedioAgente = logsAgente
                    .Where(l => l.TiempoRespuestaMs != null && l.TiempoRespuestaMs > 0)
                    .Select(l => l.TiempoRespuestaMs!.Value)
                    .DefaultIfEmpty(0)
                    .Average();

                var tokensPromedioAgente = logsAgente
                    .Where(l => l.TokensUsados != null && l.TokensUsados > 0)
                    .Select(l => l.TokensUsados!.Value)
                    .DefaultIfEmpty(0)
                    .Average();

                var similitudPromedioAgente = logsAgente
                    .Where(l => l.SimilitudPromedio != null && l.SimilitudPromedio > 0)
                    .Select(l => (double)l.SimilitudPromedio!.Value)
                    .DefaultIfEmpty(0)
                    .Average();

                var similitudMaximaAgente = logsAgente
                    .Where(l => l.SimilitudMaxima != null && l.SimilitudMaxima > 0)
                    .Select(l => (double)l.SimilitudMaxima!.Value)
                    .DefaultIfEmpty(0)
                    .Max();

                viewModel.TiempoPromedioRespuestaAgente = Math.Round(tiempoPromedioAgente, 2);
                viewModel.TokensPromedioAgente = Math.Round(tokensPromedioAgente, 2);

                viewModel.ConsultasSinResultados = logsAgente
                    .Count(l => l.CantidadResultados == 0);

                viewModel.SimilitudPromedioAgente = Math.Round(similitudPromedioAgente, 4);
                viewModel.SimilitudMaximaAgente = Math.Round(similitudMaximaAgente, 4);

                viewModel.HerramientasUsadas = logsAgente
                    .Where(l => l.HerramientaUsada != null && l.HerramientaUsada != "")
                    .GroupBy(l => l.HerramientaUsada!)
                    .Select(g => new
                    {
                        Herramienta = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Herramienta, x => x.Cantidad);

                viewModel.ConsultasPorTipo = logsAgente
                    .GroupBy(l => l.Tipo)
                    .Select(g => new
                    {
                        Tipo = g.Key.ToString(),
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Tipo, x => x.Cantidad);

                viewModel.UltimosLogsAgente = logsAgente
                    .OrderByDescending(l => l.Fecha)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando métricas de logs del agente en Admin.");
            }

            try
            {
                var logsSistema = _context.Set<LogSistema>();

                viewModel.TotalLogsSistema = logsSistema.Count();

                viewModel.LogsSistemaExitosos = logsSistema
                    .Count(l => l.Exitoso);

                viewModel.LogsSistemaError = logsSistema
                    .Count(l => !l.Exitoso);

                var latenciaPromedioSistema = logsSistema
                    .Where(l => l.LatenciaMs != null && l.LatenciaMs > 0)
                    .Select(l => l.LatenciaMs!.Value)
                    .DefaultIfEmpty(0)
                    .Average();

                viewModel.LatenciaPromedioSistema = Math.Round(latenciaPromedioSistema, 2);

                viewModel.LogsSistemaPorTipo = logsSistema
                    .GroupBy(l => l.Tipo)
                    .Select(g => new
                    {
                        Tipo = g.Key.ToString(),
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Tipo, x => x.Cantidad);

                viewModel.LogsSistemaPorEntidad = logsSistema
                    .Where(l => l.Entidad != null && l.Entidad != "")
                    .GroupBy(l => l.Entidad!)
                    .Select(g => new
                    {
                        Entidad = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Entidad, x => x.Cantidad);

                viewModel.UltimosLogsSistema = logsSistema
                    .OrderByDescending(l => l.Fecha)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando métricas de logs del sistema en Admin.");
            }

            return View(viewModel);
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