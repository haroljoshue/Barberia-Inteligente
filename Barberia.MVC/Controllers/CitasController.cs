using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModelosBarberia;
using CRUD;
using System.Security.Claims;
using ModelosBarberia.DTOs;

namespace Barberia.MVC.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        // GET: Citas
        public IActionResult Index()
        {
            var citas = CRUD<Cita>.GetAll();

            // Cliente solo ve sus citas
            if (User.IsInRole("Cliente"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                citas = citas?.Where(c => c.ClienteId == userId).ToList();
            }

            return View(citas);
        }

        // GET: Citas/Create
        [Authorize(Roles = "Cliente,Admin")]
        public IActionResult Create()
        {
            CargarDropdowns();
            return View();
        }

        // POST: Citas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente,Admin")]
        public IActionResult Create(Cita cita)
        {
            // Asigna el cliente desde la sesión
            cita.ClienteId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            cita.FechaRegistro = DateTime.UtcNow;
            cita.Estado = ModelosBarberia.Enum.EstadoCita.Pendiente;

            ModelState.Remove("ClienteId");
            ModelState.Remove("Cliente");
            ModelState.Remove("Barbero");
            ModelState.Remove("Servicio");

            if (!ModelState.IsValid)
            {
                CargarDropdowns();
                return View(cita);
            }

            CRUD<Cita>.Create(cita);
            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/Edit/5
        [Authorize(Roles = "Admin,Barbero")]
        public IActionResult Edit(string id)
        {
            var cita = CRUD<Cita>.GetById(id);
            if (cita == null) return NotFound();
            CargarDropdowns();
            return View(cita);
        }

        // POST: Citas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Barbero")]
        public IActionResult Edit(string id, Cita cita)
        {
            ModelState.Remove("Cliente");
            ModelState.Remove("Barbero");
            ModelState.Remove("Servicio");

            if (!ModelState.IsValid)
            {
                CargarDropdowns();
                return View(cita);
            }

            CRUD<Cita>.Update(id, cita);
            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/Details/5
        public IActionResult Details(string id)
        {
            var cita = CRUD<Cita>.GetById(id);
            if (cita == null) return NotFound();
            return View(cita);
        }

        // GET: Citas/Delete/5
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(string id)
        {
            var cita = CRUD<Cita>.GetById(id);
            if (cita == null) return NotFound();
            return View(cita);
        }

        // POST: Citas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(string id)
        {
            CRUD<Cita>.Delete(id);
            return RedirectToAction(nameof(Index));
        }

        private void CargarDropdowns()
        {
            var barberos = CRUD<Barbero>.GetAll()
                ?.Where(b => b.Disponible)
                .Select(b => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.Nombre} - {b.Especialidad}"
                }).ToList();

            var servicios = CRUD<Servicio>.GetAll()
                ?.Where(s => s.Activo)
                .Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Nombre} - ${s.Precio} ({s.DuracionMinutos} min)"
                }).ToList();

            ViewBag.Barberos = barberos ?? new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            ViewBag.Servicios = servicios ?? new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
        }
    }
}