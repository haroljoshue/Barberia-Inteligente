using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using ModelosBarberia;
using ModelosBarberia.ViewModels;
using ModelosBarberia.DTO_s;

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
            var clienteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            var cliente = await client.GetFromJsonAsync<ApplicationUser>($"api/usuarios/{clienteId}");

            var citas = new List<Cita>();
            var citasResponse = await client.GetAsync($"api/citas/cliente/{clienteId}");
            if (citasResponse.IsSuccessStatusCode)
                citas = await citasResponse.Content.ReadFromJsonAsync<List<Cita>>() ?? new();

            var vm = new ClienteDashboardViewModel
            {
                Cliente = cliente!,
                HistorialCitas = citas,
                CitasPendientes = citas.Count(c => c.Estado == ModelosBarberia.Enum.EstadoCita.Pendiente),
                CitasCompletadas = citas.Count(c => c.Estado == ModelosBarberia.Enum.EstadoCita.Atendida),
                // 💡 CORRECCIÓN: Usamos DateTime.UtcNow para emparejar con el formato de la API y Supabase
                ProximaCita = citas
                    .Where(c => c.FechaHora >= DateTime.UtcNow &&
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
                Barberos = barberos
                    .Where(b => b.Disponible)
                    .Select(b => new BarberoSelectItem
                    {
                        Id = b.Id,
                        Nombre = b.Nombre
                    })
                    .ToList(),
                Servicios = servicios.Where(s => s.Activo).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(AgendarCitaRequest request)
        {
            request.ClienteId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            request.FechaHora = DateTime.SpecifyKind(request.FechaHora, DateTimeKind.Utc);

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

            // Usamos el endpoint específico para cambiar estado
            var response = await client.PutAsJsonAsync(
                $"api/citas/{citaId}/estado",
                ModelosBarberia.Enum.EstadoCita.Cancelada
            );

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cita cancelada con éxito.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "No se pudo cancelar la cita.";
            return RedirectToAction(nameof(Index));
        }

    }
}