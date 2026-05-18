using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Barberia.MVC.Data;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using ModelosBarberia.Enum;

namespace Barberia.MVC.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalCitas = _context.Set<Cita>().Count();

            var citasConVector = _context.Set<Cita>()
                .Count(c => c.IdVector != null && c.IdVector != "");

            var citasSinVector = _context.Set<Cita>()
                .Count(c => c.IdVector == null || c.IdVector == "");

            var porcentajeVectorizacion = totalCitas == 0
                ? 0
                : Math.Round((double)citasConVector / totalCitas * 100, 2);

            var reservasPorServicio = _context.Servicios
                .Select(s => new
                {
                    Nombre = s.Nombre,
                    Cantidad = s.Citas.Count()
                })
                .ToList();

            var usuariosPorRol = _context.Users
                .GroupBy(u => u.RolSistema)
                .Select(g => new
                {
                    Rol = g.Key ?? "Sin Rol",
                    Cantidad = g.Count()
                })
                .ToList();

            var citasPorCliente = _context.Set<Cita>()
                .GroupBy(c => c.ClienteId)
                .Select(g => new
                {
                    Cliente = g.Key ?? "Sin cliente",
                    Cantidad = g.Count()
                })
                .ToList();

            var logsAgente = _context.Set<LogAgente>();

            var totalConsultasAgente = logsAgente.Count();

            var consultasExitosasAgente = logsAgente
                .Count(l => l.ConsultaExitosa);

            var consultasErrorAgente = logsAgente
                .Count(l => !l.ConsultaExitosa || l.MensajeError != null);

            var tiempoPromedioRespuestaAgente = logsAgente
                .Where(l => l.TiempoRespuestaMs != null && l.TiempoRespuestaMs > 0)
                .Select(l => l.TiempoRespuestaMs!.Value)
                .DefaultIfEmpty(0)
                .Average();

            var tokensPromedioAgente = logsAgente
                .Where(l => l.TokensUsados != null && l.TokensUsados > 0)
                .Select(l => l.TokensUsados!.Value)
                .DefaultIfEmpty(0)
                .Average();

            var consultasSinResultados = logsAgente
                .Count(l => l.CantidadResultados == 0);

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

            var herramientasUsadas = logsAgente
                .Where(l => l.HerramientaUsada != null && l.HerramientaUsada != "")
                .GroupBy(l => l.HerramientaUsada!)
                .Select(g => new
                {
                    Herramienta = g.Key,
                    Cantidad = g.Count()
                })
                .ToList()
                .ToDictionary(x => x.Herramienta, x => x.Cantidad);

            var consultasPorTipo = logsAgente
                .GroupBy(l => l.Tipo)
                .Select(g => new
                {
                    Tipo = g.Key.ToString(),
                    Cantidad = g.Count()
                })
                .ToList()
                .ToDictionary(x => x.Tipo, x => x.Cantidad);

            var ultimosLogsAgente = logsAgente
                .OrderByDescending(l => l.Fecha)
                .Take(10)
                .ToList();

            var viewModel = new AdminDashboardViewModel
            {
                ReservasPorServicio = reservasPorServicio
                    .ToDictionary(x => x.Nombre, x => x.Cantidad),

                UsuariosPorRol = usuariosPorRol
                    .ToDictionary(x => x.Rol, x => x.Cantidad),

                CitasPorCliente = citasPorCliente
                    .ToDictionary(x => x.Cliente, x => x.Cantidad),

                TotalCitas = totalCitas,

                CitasHoy = _context.Set<Cita>()
                    .Count(c => c.FechaHora.Date == DateTime.UtcNow.Date),

                CitasPendientes = _context.Set<Cita>()
                    .Count(c => c.Estado == EstadoCita.Pendiente),

                TotalUsuarios = _context.Users.Count(),

                TotalBarberos = _context.Set<Barbero>()
                    .Count(b => b.Disponible),

                TotalServicios = _context.Set<Servicio>()
                    .Count(s => s.Activo),

                CitasConVector = citasConVector,
                CitasSinVector = citasSinVector,
                PorcentajeVectorizacion = porcentajeVectorizacion,
                DimensionEmbedding = 1536,

                TotalConsultasAgente = totalConsultasAgente,
                ConsultasExitosasAgente = consultasExitosasAgente,
                ConsultasErrorAgente = consultasErrorAgente,
                TiempoPromedioRespuestaAgente = Math.Round(tiempoPromedioRespuestaAgente, 2),
                TokensPromedioAgente = Math.Round(tokensPromedioAgente, 2),
                ConsultasSinResultados = consultasSinResultados,
                SimilitudPromedioAgente = Math.Round(similitudPromedioAgente, 4),
                SimilitudMaximaAgente = Math.Round(similitudMaximaAgente, 4),

                HerramientasUsadas = herramientasUsadas,
                ConsultasPorTipo = consultasPorTipo,
                UltimosLogsAgente = ultimosLogsAgente
            };

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
    }
}