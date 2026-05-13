using Microsoft.AspNetCore.Mvc;
using Barberia.MVC.Data; // tu contexto
using System.Linq;
using ModelosBarberia;

namespace Barberia.MVC.Controllers
{
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
                    Rol = g.Key,
                    Cantidad = g.Count()
                }).ToList();

            var viewModel = new AdminDashboardViewModel
            {
                ReservasPorServicio = reservasPorServicio
                    .ToDictionary(x => x.Nombre, x => x.Cantidad),
                UsuariosPorRol = usuariosPorRol
                    .ToDictionary(x => x.Rol, x => x.Cantidad)
            };

            return View(viewModel);
        }
    }

    public class AdminDashboardViewModel
    {
        public Dictionary<string, int> ReservasPorServicio { get; set; }
        public Dictionary<string, int> UsuariosPorRol { get; set; }
    }
}
