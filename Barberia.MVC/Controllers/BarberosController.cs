using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModelosBarberia;
using CRUD;

namespace Barberia.MVC.Controllers
{
    public class BarberosController : Controller
    {
        // Visible para todos
        public IActionResult Index()
        {
            var barberos = CRUD<Barbero>.GetAll();
            return View(barberos);
        }

        public IActionResult Barberos()
        {
            var barberos = CRUD<Barbero>.GetAll();
            return View(barberos);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Details(string id)
        {
            var barbero = CRUD<Barbero>.GetById(id);
            if (barbero == null) return NotFound();
            return View(barbero);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Create(Barbero barbero)
        {
            barbero.FechaRegistro = DateTime.UtcNow;
            ModelState.Remove("Citas");
            if (!ModelState.IsValid) return View(barbero);
            CRUD<Barbero>.Create(barbero);
            TempData["Success"] = "Barbero registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(string id)
        {
            var barbero = CRUD<Barbero>.GetById(id);
            if (barbero == null) return NotFound();
            return View(barbero);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(string id, Barbero barbero)
        {
            ModelState.Remove("Citas");
            if (!ModelState.IsValid) return View(barbero);
            CRUD<Barbero>.Update(id, barbero);
            TempData["Success"] = "Barbero actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(string id)
        {
            var barbero = CRUD<Barbero>.GetById(id);
            if (barbero == null) return NotFound();
            return View(barbero);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(string id)
        {
            CRUD<Barbero>.Delete(id);
            TempData["Success"] = "Barbero eliminado.";
            return RedirectToAction(nameof(Index));
        }
    }
}