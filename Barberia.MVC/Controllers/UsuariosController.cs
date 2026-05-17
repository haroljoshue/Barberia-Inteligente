using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using ModelosBarberia;
using Microsoft.AspNetCore.Authorization;

namespace Barberia.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UsuariosController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var usuarios = await client.GetFromJsonAsync<List<ApplicationUser>>("api/usuarios");
            return View(usuarios);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var usuario = await client.GetFromJsonAsync<ApplicationUser>($"api/usuarios/{id}");
            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser usuario, string password)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.PostAsJsonAsync($"api/usuarios/register?password={password}", usuario);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al crear usuario");
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var usuario = await client.GetFromJsonAsync<ApplicationUser>($"api/usuarios/{id}");
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser usuario)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.PutAsJsonAsync($"api/usuarios/{id}", usuario);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al editar usuario");
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var usuario = await client.GetFromJsonAsync<ApplicationUser>($"api/usuarios/{id}");
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.DeleteAsync($"api/usuarios/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al eliminar usuario");
            return View();
        }
    }
}
