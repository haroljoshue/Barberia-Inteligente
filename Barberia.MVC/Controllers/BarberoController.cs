using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelosBarberia;
using ModelosBarberia.Enum;
using ModelosBarberia.ViewModels;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Barberia.MVC.Controllers
{
    [Authorize(Roles = "Barbero,Admin")]
    public class BarberoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BarberoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var barberoIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int barberoId = int.Parse(barberoIdStr!);

            var client = _httpClientFactory.CreateClient("BarberiaApi");

            var citas = await client.GetFromJsonAsync<List<Cita>>
                ($"api/citas/barbero/{barberoId}") ?? new();

            var dashboard = new BarberoDashboardViewModel
            {
                Citas = citas,
                CitasHoy = citas.Count(c => c.FechaHora.Date == DateTime.Today),
                CitasPendientes = citas.Count(c => c.Estado == EstadoCita.Pendiente),
                CitasCompletadas = citas.Count(c => c.Estado == EstadoCita.Atendida),
                TopClientes = new List<ApplicationUser>()
            };

            return View(dashboard);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int citaId, EstadoCita nuevoEstado)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            var response = await client.PutAsJsonAsync(
                $"api/citas/{citaId}/estado",
                nuevoEstado);

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                response.IsSuccessStatusCode
                    ? "Estado actualizado correctamente."
                    : "Error al actualizar estado.";

            return RedirectToAction(nameof(Index));
        }
    }
}