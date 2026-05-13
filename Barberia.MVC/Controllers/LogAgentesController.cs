using Microsoft.AspNetCore.Mvc;
using ModelosBarberia;
using CRUD;

namespace Barberia.MVC.Controllers
{
    public class LogAgentesController : Controller
    {
        // GET: LogAgentes
        public IActionResult Index()
        {
            var logs = CRUD<LogAgente>.GetAll();
            return View(logs);
        }

        // GET: LogAgentes/Details/5
        public IActionResult Details(string id)
        {
            var log = CRUD<LogAgente>.GetById(id);
            if (log == null)
                return NotFound();

            return View(log);
        }

        // GET: LogAgentes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LogAgentes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LogAgente logAgente)
        {
            if (!ModelState.IsValid) return View(logAgente);

            CRUD<LogAgente>.Create(logAgente);
            return RedirectToAction(nameof(Index));
        }

        // GET: LogAgentes/Edit/5
        public IActionResult Edit(string id)
        {
            var log = CRUD<LogAgente>.GetById(id);
            if (log == null)
                return NotFound();

            return View(log);
        }

        // POST: LogAgentes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, LogAgente logAgente)
        {
            if (!ModelState.IsValid) return View(logAgente);

            CRUD<LogAgente>.Update(id, logAgente);
            return RedirectToAction(nameof(Index));
        }

        // GET: LogAgentes/Delete/5
        public IActionResult Delete(string id)
        {
            var log = CRUD<LogAgente>.GetById(id);
            if (log == null)
                return NotFound();

            return View(log);
        }

        // POST: LogAgentes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            CRUD<LogAgente>.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
