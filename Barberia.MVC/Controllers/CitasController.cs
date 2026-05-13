using Microsoft.AspNetCore.Mvc;
using ModelosBarberia;
using CRUD;

namespace Barberia.MVC.Controllers
{
    public class CitasController : Controller
    {
        // GET: Citas
        public IActionResult Index()
        {
            var citas = CRUD<Cita>.GetAll();
            return View(citas);
        }

        // GET: Citas/Details/5
        public IActionResult Details(string id)
        {
            var cita = CRUD<Cita>.GetById(id);
            if (cita == null)
                return NotFound();

            return View(cita);
        }

        // GET: Citas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Citas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cita cita)
        {
            if (!ModelState.IsValid) return View(cita);

            CRUD<Cita>.Create(cita);
            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/Edit/5
        public IActionResult Edit(string id)
        {
            var cita = CRUD<Cita>.GetById(id);
            if (cita == null)
                return NotFound();

            return View(cita);
        }

        // POST: Citas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Cita cita)
        {
            if (!ModelState.IsValid) return View(cita);

            CRUD<Cita>.Update(id, cita);
            return RedirectToAction(nameof(Index));
        }

        // GET: Citas/Delete/5
        public IActionResult Delete(string id)
        {
            var cita = CRUD<Cita>.GetById(id);
            if (cita == null)
                return NotFound();

            return View(cita);
        }

        // POST: Citas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            CRUD<Cita>.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
