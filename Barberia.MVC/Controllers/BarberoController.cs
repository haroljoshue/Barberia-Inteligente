using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using ModelosBarberia;
using ModelosBarberia.Enum;
using ModelosBarberia.ViewModels;

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
            var barberoId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            var citas = await client.GetFromJsonAsync<List<Cita>>($"api/citas/barbero/{barberoId}")
                        ?? new List<Cita>();
            var topClientes = await client.GetFromJsonAsync<List<ApplicationUser>>($"api/usuarios/top-clientes/{barberoId}")
                              ?? new List<ApplicationUser>();

            var hoy = DateTime.Today;

            var dashboard = new BarberoDashboardViewModel
            {
                Citas = citas,
                TopClientes = topClientes,
                CitasHoy = citas.Count(c => c.FechaHora.Year == hoy.Year && c.FechaHora.Month == hoy.Month && c.FechaHora.Day == hoy.Day),
                CitasPendientes = citas.Count(c => c.Estado == EstadoCita.Pendiente),
                CitasCompletadas = citas.Count(c => c.Estado == EstadoCita.Atendida),
            };

            return View(dashboard);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int citaId, EstadoCita nuevoEstado)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");
            var response = await client.PutAsJsonAsync($"api/citas/{citaId}/estado", nuevoEstado);

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] = response.IsSuccessStatusCode
                ? "Estado actualizado correctamente."
                : "No se pudo cambiar el estado.";

            return RedirectToAction(nameof(Index));
        }
    }
}