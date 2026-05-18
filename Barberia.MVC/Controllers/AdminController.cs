using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Barberia.MVC.Data;
using ModelosBarberia;

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
            return Content("ADMIN CARGÓ CORRECTAMENTE");
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
    }
}