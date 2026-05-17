using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Barberia.MVC.Controllers
{
    public class AsistenteController : Controller
    {
        // GET: AsistenteController
        public ActionResult Index()
        {
            return View();
        }

        // GET: AsistenteController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AsistenteController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AsistenteController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AsistenteController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AsistenteController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AsistenteController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AsistenteController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
