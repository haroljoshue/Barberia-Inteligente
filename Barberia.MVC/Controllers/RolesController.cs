using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;

namespace Barberia.MVC.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RolesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RolesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var roles = await client.GetFromJsonAsync<List<IdentityRole>>("api/roles");
            return View(roles);
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var rol = await client.GetFromJsonAsync<IdentityRole>($"api/roles/{id}");
            return View(rol);
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nombreRol)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.PostAsJsonAsync("api/roles/crear", nombreRol);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al crear rol");
            return View();
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var rol = await client.GetFromJsonAsync<IdentityRole>($"api/roles/{id}");
            return View(rol);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, IdentityRole rol)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.PutAsJsonAsync($"api/roles/{id}", rol);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al editar rol");
            return View(rol);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var rol = await client.GetFromJsonAsync<IdentityRole>($"api/roles/{id}");
            return View(rol);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.DeleteAsync($"api/roles/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al eliminar rol");
            return View();
        }
    }
}
