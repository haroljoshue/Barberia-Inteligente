using Microsoft.AspNetCore.Mvc;
using ModelosBarberia;
using CRUD;

namespace Barberia.MVC.Controllers
{
    public class BarberosController : Controller
    {
        // GET: Barberos
        public IActionResult Index()
        {
            var barberos = CRUD<Barbero>.GetAll();
            return View(barberos);
        }

        // GET: Barberos
        public IActionResult Barberos()
        {
            var barberos = CRUD<Barbero>.GetAll();
            return View(barberos);
        }

        // GET: Barberos/Details/5
        public IActionResult Details(string id)
        {
            var barbero = CRUD<Barbero>.GetById(id);
            if (barbero == null)
                return NotFound();

            return View(barbero);
        }

        // GET: Barberos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Barberos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Barbero barbero)
        {
            if (!ModelState.IsValid) return View(barbero);

            CRUD<Barbero>.Create(barbero);
            return RedirectToAction(nameof(Index));
        }

        // GET: Barberos/Edit/5
        public IActionResult Edit(string id)
        {
            var barbero = CRUD<Barbero>.GetById(id);
            if (barbero == null)
                return NotFound();

            return View(barbero);
        }

        // POST: Barberos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Barbero barbero)
        {
            if (!ModelState.IsValid) return View(barbero);

            CRUD<Barbero>.Update(id, barbero);
            return RedirectToAction(nameof(Index));
        }

        // GET: Barberos/Delete/5
        public IActionResult Delete(string id)
        {
            var barbero = CRUD<Barbero>.GetById(id);
            if (barbero == null)
                return NotFound();

            return View(barbero);
        }

        // POST: Barberos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            CRUD<Barbero>.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
