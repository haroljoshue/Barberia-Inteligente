using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            try
            {
                var barberoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(barberoId))
                    return Unauthorized();

                var client = _httpClientFactory.CreateClient("BarberiaApi");

                var citas = await client.GetFromJsonAsync<List<Cita>>($"api/citas/barbero/{barberoId}") ?? new List<Cita>();
                var topClientes = await client.GetFromJsonAsync<List<ApplicationUser>>($"api/usuarios/top-clientes/{barberoId}") ?? new List<ApplicationUser>();

                var hoy = DateTime.Today;

                var dashboard = new BarberoDashboardViewModel
                {
                    Citas = citas,
                    TopClientes = topClientes,
                    CitasHoy = citas.Count(c => c.FechaHora.Date == hoy),
                    CitasPendientes = citas.Count(c => c.Estado == EstadoCita.Pendiente),
                    CitasCompletadas = citas.Count(c => c.Estado == EstadoCita.Atendida)
                };

                return View(dashboard);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el panel: {ex.Message}";
                return View(new BarberoDashboardViewModel
                {
                    Citas = new List<Cita>(),
                    TopClientes = new List<ApplicationUser>()
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int citaId, EstadoCita nuevoEstado)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("BarberiaApi");
                var response = await client.PutAsJsonAsync($"api/citas/{citaId}/estado", nuevoEstado);

                TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                    response.IsSuccessStatusCode ? "Estado actualizado correctamente." : "No se pudo cambiar el estado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar estado: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}