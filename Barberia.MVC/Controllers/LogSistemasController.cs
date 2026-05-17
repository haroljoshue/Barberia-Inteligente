using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModelosBarberia;
using CRUD;

namespace Barberia.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LogSistemasController : Controller
    {
        public IActionResult Index()
        {
            var logs = CRUD<LogSistema>.GetAll();
            return View(logs);
        }

        public IActionResult Details(string id)
        {
            var log = CRUD<LogSistema>.GetById(id);
            if (log == null) return NotFound();
            return View(log);
        }
    }
}