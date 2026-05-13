using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using ModelosBarberia;
using ModelosBarberia.Enum;
using ModelosBarberia.ViewModels;

namespace Barberia.MVC.Controllers
{
    public class BarberoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BarberoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Barbero/Index
        public async Task<IActionResult> Index(string barberoId)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            // Citas del barbero
            var citas = await client.GetFromJsonAsync<List<Cita>>($"api/citas/barbero/{barberoId}");

            // Top clientes del barbero (usuarios con rol Cliente)
            var topClientes = await client.GetFromJsonAsync<List<ApplicationUser>>($"api/usuarios/top-clientes/{barberoId}");

            var dashboard = new BarberoDashboardViewModel
            {
                Citas = citas ?? new List<Cita>(),
                TopClientes = topClientes ?? new List<ApplicationUser>()
            };

            return View(dashboard);
        }

        // POST: Barbero/CambiarEstado
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int citaId, EstadoCita nuevoEstado)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.PutAsJsonAsync($"api/citas/{citaId}/estado", nuevoEstado);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            TempData["Error"] = "No se pudo cambiar el estado de la cita.";
            return RedirectToAction(nameof(Index));
        }
    }
}
