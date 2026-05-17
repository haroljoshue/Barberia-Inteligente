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
            // Obtiene el ID del barbero desde la sesión
            var barberoId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            var citas = await client.GetFromJsonAsync<List<Cita>>($"api/citas/barbero/{barberoId}")
                        ?? new List<Cita>();
            var topClientes = await client.GetFromJsonAsync<List<ApplicationUser>>($"api/usuarios/top-clientes/{barberoId}")
                              ?? new List<ApplicationUser>();

            var dashboard = new BarberoDashboardViewModel
            {
                Citas = citas,
                TopClientes = topClientes,
                CitasHoy = citas.Count(c => c.FechaHora.Date == DateTime.Today),
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