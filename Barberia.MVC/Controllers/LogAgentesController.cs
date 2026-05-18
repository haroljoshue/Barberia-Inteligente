using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ModelosBarberia;
using CRUD;

namespace Barberia.MVC.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class LogAgentesController : Controller
    {
        public IActionResult Index()
        {
            var logs = CRUD<LogAgente>.GetAll();
            return View(logs);
        }

        public IActionResult Details(string id)
        {
            var log = CRUD<LogAgente>.GetById(id);
            if (log == null) return NotFound();
            return View(log);
        }
    }
}