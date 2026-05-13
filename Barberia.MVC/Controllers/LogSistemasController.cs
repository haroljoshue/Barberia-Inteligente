using Microsoft.AspNetCore.Mvc;
using ModelosBarberia;
using CRUD;

namespace Barberia.MVC.Controllers
{
    public class LogSistemasController : Controller
    {
        // GET: LogSistemas
        public IActionResult Index()
        {
            var logs = CRUD<LogSistema>.GetAll();
            return View(logs);
        }

        // GET: LogSistemas/Details/5
        public IActionResult Details(string id)
        {
            var log = CRUD<LogSistema>.GetById(id);
            if (log == null)
                return NotFound();

            return View(log);
        }

        // GET: LogSistemas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LogSistemas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LogSistema logSistema)
        {
            if (!ModelState.IsValid) return View(logSistema);

            CRUD<LogSistema>.Create(logSistema);
            return RedirectToAction(nameof(Index));
        }

        // GET: LogSistemas/Edit/5
        public IActionResult Edit(string id)
        {
            var log = CRUD<LogSistema>.GetById(id);
            if (log == null)
                return NotFound();

            return View(log);
        }

        // POST: LogSistemas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, LogSistema logSistema)
        {
            if (!ModelState.IsValid) return View(logSistema);

            CRUD<LogSistema>.Update(id, logSistema);
            return RedirectToAction(nameof(Index));
        }

        // GET: LogSistemas/Delete/5
        public IActionResult Delete(string id)
        {
            var log = CRUD<LogSistema>.GetById(id);
            if (log == null)
                return NotFound();

            return View(log);
        }

        // POST: LogSistemas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            CRUD<LogSistema>.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
