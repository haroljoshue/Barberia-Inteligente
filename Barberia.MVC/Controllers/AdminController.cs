using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Barberia.MVC.Data;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using ModelosBarberia.Enum;

namespace Barberia.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var reservasPorServicio = _context.Servicios
                .Select(s => new {
                    Nombre = s.Nombre,
                    Cantidad = s.Citas.Count()
                }).ToList();

            var usuariosPorRol = _context.Users
                .GroupBy(u => u.RolSistema)
                .Select(g => new {
                    Rol = g.Key ?? "Sin Rol",
                    Cantidad = g.Count()
                }).ToList();

            var viewModel = new AdminDashboardViewModel
            {
                ReservasPorServicio = reservasPorServicio
                    .ToDictionary(x => x.Nombre, x => x.Cantidad),
                UsuariosPorRol = usuariosPorRol
                    .ToDictionary(x => x.Rol, x => x.Cantidad),
                TotalCitas = _context.Set<Cita>().Count(),
                CitasHoy = _context.Set<Cita>()
                    .Count(c => c.FechaHora.Date == DateTime.UtcNow.Date),
                CitasPendientes = _context.Set<Cita>()
                    .Count(c => c.Estado == EstadoCita.Pendiente),
                TotalUsuarios = _context.Users.Count(),
                TotalBarberos = _context.Set<Barbero>().Count(b => b.Disponible),
                TotalServicios = _context.Set<Servicio>().Count(s => s.Activo),
            };

            return View(viewModel);
        }
    }

    public class AdminDashboardViewModel
    {
        public Dictionary<string, int> ReservasPorServicio { get; set; } = new();
        public Dictionary<string, int> UsuariosPorRol { get; set; } = new();
        public int TotalCitas { get; set; }
        public int CitasHoy { get; set; }
        public int CitasPendientes { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalBarberos { get; set; }
        public int TotalServicios { get; set; }
    }
}