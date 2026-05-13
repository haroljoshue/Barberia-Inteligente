using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using ModelosBarberia;
using ModelosBarberia.Enum;
using ModelosBarberia.ViewModels;

namespace Barberia.MVC.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ClienteController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Cliente/Index
        public async Task<IActionResult> Index(string clienteId)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            // Obtener datos del cliente
            var cliente = await client.GetFromJsonAsync<ApplicationUser>($"api/usuarios/{clienteId}");

            // Obtener historial de citas
            var citas = await client.GetFromJsonAsync<List<Cita>>($"api/citas/cliente/{clienteId}");

            var vm = new ClienteDashboardViewModel
            {
                Cliente = cliente!,
                HistorialCitas = citas ?? new List<Cita>()
            };

            return View(vm);
        }

        // GET: Cliente/Agendar
        public async Task<IActionResult> Agendar()
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            var barberos = await client.GetFromJsonAsync<List<ApplicationUser>>("api/usuarios/barberos");
            var servicios = await client.GetFromJsonAsync<List<Servicio>>("api/servicios");

            var vm = new AgendarCitaViewModel
            {
                Barberos = barberos ?? new List<ApplicationUser>(),
                Servicios = servicios ?? new List<Servicio>()
            };

            return View(vm);
        }

        // POST: Cliente/Agendar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(AgendarCitaRequest request)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.PostAsJsonAsync("api/citas", request);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { clienteId = request.ClienteId });

            TempData["Error"] = "No se pudo agendar la cita.";
            return RedirectToAction(nameof(Agendar));
        }
    }
}