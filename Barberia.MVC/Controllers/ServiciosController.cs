using Microsoft.AspNetCore.Mvc;
using ModelosBarberia;
using CRUD;

namespace Barberia.MVC.Controllers
{
    public class ServiciosController : Controller
    {
        // GET: Servicios
        public IActionResult Index()
        {
            var servicios = CRUD<Servicio>.GetAll();
            return View(servicios);
        }

        // GET: Servicios
        public IActionResult Servicios()
        {
            var servicios = CRUD<Servicio>.GetAll();
            return View(servicios);
        }

        // GET: Servicios/Details/5
        public IActionResult Details(string id)
        {
            var servicio = CRUD<Servicio>.GetById(id);
            if (servicio == null)
                return NotFound();

            return View(servicio);
        }

        // GET: Servicios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Servicios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Servicio servicio)
        {
            if (!ModelState.IsValid) return View(servicio);

            CRUD<Servicio>.Create(servicio);
            return RedirectToAction(nameof(Index));
        }

        // GET: Servicios/Edit/5
        public IActionResult Edit(string id)
        {
            var servicio = CRUD<Servicio>.GetById(id);
            if (servicio == null)
                return NotFound();

            return View(servicio);
        }

        // POST: Servicios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Servicio servicio)
        {
            if (!ModelState.IsValid) return View(servicio);

            CRUD<Servicio>.Update(id, servicio);
            return RedirectToAction(nameof(Index));
        }

        // GET: Servicios/Delete/5
        public IActionResult Delete(string id)
        {
            var servicio = CRUD<Servicio>.GetById(id);
            if (servicio == null)
                return NotFound();

            return View(servicio);
        }

        // POST: Servicios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            CRUD<Servicio>.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
