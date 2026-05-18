using Barberia.MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            // =========================
            // CITAS
            // =========================
            try
            {
                viewModel.TotalCitas = _context.Citas.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando total de citas.");
            }

            try
            {
                var hoyInicio = DateTime.Today;
                var mananaInicio = hoyInicio.AddDays(1);

                viewModel.CitasHoy = _context.Citas.Count(c =>
                    c.FechaHora >= hoyInicio &&
                    c.FechaHora < mananaInicio
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando citas de hoy.");
            }

            try
            {
                viewModel.CitasPendientes = _context.Citas.Count(c =>
                    c.Estado == EstadoCita.Pendiente
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando citas pendientes.");
            }

            try
            {
                // Si IdVector es string?
                viewModel.CitasConVector = _context.Citas.Count(c =>
                    c.IdVector != null && c.IdVector.Trim() != ""
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando citas con vector.");
            }

            try
            {
                // Si IdVector es string?
                viewModel.CitasSinVector = _context.Citas.Count(c =>
                    c.IdVector == null || c.IdVector.Trim() == ""
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando citas sin vector.");
            }

            try
            {
                viewModel.PorcentajeVectorizacion = viewModel.TotalCitas == 0
                    ? 0
                    : Math.Round((double)viewModel.CitasConVector / viewModel.TotalCitas * 100, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando porcentaje de vectorización.");
            }

            try
            {
                viewModel.CitasPorCliente = _context.Citas
                    .GroupBy(c => c.ClienteId)
                    .Select(g => new
                    {
                        Cliente = string.IsNullOrEmpty(g.Key) ? "Sin cliente" : g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Cliente, x => x.Cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agrupando citas por cliente.");
            }

            // =========================
            // USUARIOS
            // =========================
            try
            {
                viewModel.TotalUsuarios = _context.Users.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando usuarios.");
            }

            try
            {
                viewModel.UsuariosPorRol = _context.Users
                    .GroupBy(u => u.RolSistema)
                    .Select(g => new
                    {
                        Rol = string.IsNullOrEmpty(g.Key) ? "Sin Rol" : g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Rol, x => x.Cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agrupando usuarios por rol.");
            }

            // =========================
            // BARBEROS
            // =========================
            try
            {
                // Cuenta TODOS los barberos, no solo disponibles.
                viewModel.TotalBarberos = _context.Barberos.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando barberos.");
            }

            // =========================
            // SERVICIOS
            // =========================
            try
            {
                // Cuenta TODOS los servicios, no solo activos.
                viewModel.TotalServicios = _context.Servicios.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando servicios.");
            }

            try
            {
                viewModel.ReservasPorServicio = _context.Citas
                    .Join(
                        _context.Servicios,
                        cita => cita.ServicioId,
                        servicio => servicio.Id,
                        (cita, servicio) => new
                        {
                            Servicio = servicio.Nombre
                        }
                    )
                    .GroupBy(x => x.Servicio)
                    .Select(g => new
                    {
                        Nombre = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Nombre, x => x.Cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando reservas por servicio.");
            }

            // =========================
            // LOGS DEL AGENTE
            // =========================
            try
            {
                viewModel.TotalConsultasAgente = _context.LogsAgente.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando logs del agente.");
            }

            try
            {
                viewModel.ConsultasExitosasAgente = _context.LogsAgente
                    .Count(l => l.ConsultaExitosa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando consultas exitosas del agente.");
            }

            try
            {
                viewModel.ConsultasErrorAgente = _context.LogsAgente
                    .Count(l => !l.ConsultaExitosa || l.MensajeError != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando consultas con error del agente.");
            }

            try
            {
                viewModel.ConsultasSinResultados = _context.LogsAgente
                    .Count(l => l.CantidadResultados.HasValue && l.CantidadResultados.Value == 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando consultas sin resultados.");
            }

            try
            {
                var tiempos = _context.LogsAgente
                    .Where(l => l.TiempoRespuestaMs.HasValue && l.TiempoRespuestaMs.Value > 0)
                    .Select(l => (double)l.TiempoRespuestaMs!.Value)
                    .ToList();

                viewModel.TiempoPromedioRespuestaAgente = PromedioSeguro(tiempos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando tiempo promedio del agente.");
            }

            try
            {
                var tokens = _context.LogsAgente
                    .Where(l => l.TokensUsados.HasValue && l.TokensUsados.Value > 0)
                    .Select(l => (double)l.TokensUsados!.Value)
                    .ToList();

                viewModel.TokensPromedioAgente = PromedioSeguro(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando tokens promedio del agente.");
            }

            try
            {
                var similitudes = _context.LogsAgente
                    .Where(l => l.SimilitudPromedio.HasValue && l.SimilitudPromedio.Value > 0)
                    .Select(l => (double)l.SimilitudPromedio!.Value)
                    .ToList();

                viewModel.SimilitudPromedioAgente = PromedioSeguro(similitudes, 4);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando similitud promedio del agente.");
            }

            try
            {
                var similitudesMaximas = _context.LogsAgente
                    .Where(l => l.SimilitudMaxima.HasValue && l.SimilitudMaxima.Value > 0)
                    .Select(l => (double)l.SimilitudMaxima!.Value)
                    .ToList();

                viewModel.SimilitudMaximaAgente = MaximoSeguro(similitudesMaximas, 4);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando similitud máxima del agente.");
            }

            try
            {
                viewModel.HerramientasUsadas = _context.LogsAgente
                    .Where(l => l.HerramientaUsada != null && l.HerramientaUsada != "")
                    .GroupBy(l => l.HerramientaUsada!)
                    .Select(g => new
                    {
                        Herramienta = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Herramienta, x => x.Cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agrupando herramientas usadas.");
            }

            try
            {
                viewModel.ConsultasPorTipo = _context.LogsAgente
                    .GroupBy(l => l.Tipo)
                    .Select(g => new
                    {
                        Tipo = g.Key.ToString(),
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Tipo, x => x.Cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agrupando consultas por tipo.");
            }

            try
            {
                viewModel.UltimosLogsAgente = _context.LogsAgente
                    .OrderByDescending(l => l.Fecha)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando últimos logs del agente.");
            }

            // =========================
            // LOGS DEL SISTEMA
            // =========================
            try
            {
                viewModel.TotalLogsSistema = _context.LogsSistema.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando logs del sistema.");
            }

            try
            {
                viewModel.LogsSistemaExitosos = _context.LogsSistema.Count(l => l.Exitoso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando logs exitosos del sistema.");
            }

            try
            {
                viewModel.LogsSistemaError = _context.LogsSistema.Count(l => !l.Exitoso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando logs con error del sistema.");
            }

            try
            {
                var latencias = _context.LogsSistema
                    .Where(l => l.LatenciaMs.HasValue && l.LatenciaMs.Value > 0)
                    .Select(l => (double)l.LatenciaMs!.Value)
                    .ToList();

                viewModel.LatenciaPromedioSistema = PromedioSeguro(latencias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando latencia promedio del sistema.");
            }

            try
            {
                viewModel.LogsSistemaPorTipo = _context.LogsSistema
                    .GroupBy(l => l.Tipo)
                    .Select(g => new
                    {
                        Tipo = g.Key.ToString(),
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Tipo, x => x.Cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agrupando logs del sistema por tipo.");
            }

            try
            {
                viewModel.LogsSistemaPorEntidad = _context.LogsSistema
                    .Where(l => l.Entidad != null && l.Entidad != "")
                    .GroupBy(l => l.Entidad!)
                    .Select(g => new
                    {
                        Entidad = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Entidad, x => x.Cantidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agrupando logs del sistema por entidad.");
            }

            try
            {
                viewModel.UltimosLogsSistema = _context.LogsSistema
                    .OrderByDescending(l => l.Fecha)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cargando últimos logs del sistema.");
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

        public int TotalLogsSistema { get; set; }
        public int LogsSistemaExitosos { get; set; }
        public int LogsSistemaError { get; set; }
        public double LatenciaPromedioSistema { get; set; }

        public Dictionary<string, int> LogsSistemaPorTipo { get; set; } = new();
        public Dictionary<string, int> LogsSistemaPorEntidad { get; set; } = new();
        public List<LogSistema> UltimosLogsSistema { get; set; } = new();
    }
}