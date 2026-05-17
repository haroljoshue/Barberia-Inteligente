using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using ModelosBarberia;
using ModelosBarberia.ViewModels;

namespace Barberia.MVC.Controllers
{
    [Authorize(Roles = "Cliente,Admin")]
    public class ClienteController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ClienteController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            // Cliente desde sesión, no desde parámetro
            var clienteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            var cliente = await client.GetFromJsonAsync<ApplicationUser>($"api/usuarios/{clienteId}");
            var citas = await client.GetFromJsonAsync<List<Cita>>($"api/citas/cliente/{clienteId}")
                        ?? new List<Cita>();

            var vm = new ClienteDashboardViewModel
            {
                Cliente = cliente!,
                HistorialCitas = citas,
                CitasPendientes = citas.Count(c => c.Estado == ModelosBarberia.Enum.EstadoCita.Pendiente),
                CitasCompletadas = citas.Count(c => c.Estado == ModelosBarberia.Enum.EstadoCita.Completada),
                ProximaCita = citas
                    .Where(c => c.FechaHora >= DateTime.Now &&
                                c.Estado != ModelosBarberia.Enum.EstadoCita.Cancelada)
                    .OrderBy(c => c.FechaHora)
                    .FirstOrDefault()
            };

            return View(vm);
        }

        public async Task<IActionResult> Agendar()
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var barberos = await client.GetFromJsonAsync<List<Barbero>>("api/barberos") ?? new();
            var servicios = await client.GetFromJsonAsync<List<Servicio>>("api/servicios") ?? new();

            var vm = new AgendarCitaViewModel
            {
                Barberos = barberos.Where(b => b.Disponible).ToList(),
                Servicios = servicios.Where(s => s.Activo).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(AgendarCitaRequest request)
        {
            // Asigna el cliente desde la sesión
            request.ClienteId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.PostAsJsonAsync("api/citas", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "¡Cita agendada correctamente!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "No se pudo agendar la cita. Intenta de nuevo.";
            return RedirectToAction(nameof(Agendar));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int citaId)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.PutAsJsonAsync(
                $"api/citas/{citaId}/estado",
                ModelosBarberia.Enum.EstadoCita.Cancelada);

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
                ? "Cita cancelada."
                : "No se pudo cancelar la cita.";

            return RedirectToAction(nameof(Index));
        }
    }
}