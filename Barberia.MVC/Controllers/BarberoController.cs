using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelosBarberia;
using ModelosBarberia.DTOs; // <--- Importante para reconocer CitaBarberoDto
using ModelosBarberia.Enum;
using ModelosBarberia.ViewModels;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Barberia.MVC.Controllers
{
    [Authorize]
    public class BarberoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BarberoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Authorize(Roles = "Barbero")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            // Buscar el barbero actual por UserId
            var barberos = await client.GetFromJsonAsync<List<Barbero>>("api/barberos") ?? new();
            var barberoActual = barberos.FirstOrDefault(b => b.UserId == userId);

            if (barberoActual == null)
            {
                TempData["Error"] = "No se encontró el perfil de barbero asociado a esta cuenta.";
                return RedirectToAction("Index", "Home");
            }

            int barberoId = barberoActual.Id;

            // Obtener citas del barbero
            var citasDto = await client.GetFromJsonAsync<List<CitaBarberoDto>>($"api/citas/barbero/{barberoId}") ?? new();

            var citasMapeadas = citasDto.Select(dto => new Cita
            {
                Id = dto.Id,
                FechaHora = dto.FechaHora,
                Estado = (EstadoCita)dto.Estado,
                Observacion = dto.Observacion,
                PrecioFinal = dto.PrecioFinal,
                Cliente = new ApplicationUser { NombreCompleto = dto.ClienteNombre },
                Servicio = new Servicio { Nombre = dto.ServicioNombre }
            }).ToList();

            // Calcular métricas
            var totalCitas = citasMapeadas.Count;
            var citasPendientes = citasMapeadas.Count(c => c.Estado == EstadoCita.Pendiente);
            var citasCompletadas = citasMapeadas.Count(c => c.Estado == EstadoCita.Atendida);
            var citasCanceladas = citasMapeadas.Count(c => c.Estado == EstadoCita.Cancelada);
            var gananciasTotales = citasMapeadas
                .Where(c => c.Estado == EstadoCita.Atendida)
                .Sum(c => c.PrecioFinal ?? 0); // <-- conversión segura

            var citasPorMes = citasMapeadas
                .GroupBy(c => new { c.FechaHora.Year, c.FechaHora.Month })
                .Select(g => new BarberoDashboardViewModel.CitasMes
                {
                    Mes = $"{g.Key.Month}/{g.Key.Year}",
                    Total = g.Count(),
                    Ganancias = g.Where(c => c.Estado == EstadoCita.Atendida)
                                 .Sum(c => c.PrecioFinal ?? 0) // <-- conversión segura
                }).ToList();

            var dashboard = new BarberoDashboardViewModel
            {
                Citas = citasMapeadas,
                CitasHoy = citasMapeadas.Count(c => c.FechaHora.Date == DateTime.Today),
                CitasPendientes = citasPendientes,
                CitasCompletadas = citasCompletadas,
                CitasCanceladas = citasCanceladas,
                TotalCitas = totalCitas,
                GananciasTotales = gananciasTotales,
                CitasPorMes = citasPorMes,
                TopClientes = new List<ApplicationUser>() // puedes calcular top clientes si quieres
            };

            return View(dashboard);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Barbero")]
        public async Task<IActionResult> CambiarEstado(int citaId, EstadoCita nuevoEstado)
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            // Enviamos el estado correctamente en formato JSON hacia el PUT de la API
            var response = await client.PutAsJsonAsync($"api/citas/{citaId}/estado", nuevoEstado);

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                response.IsSuccessStatusCode
                    ? "Estado actualizado correctamente."
                    : "Error al actualizar estado.";

            return RedirectToAction(nameof(Index));
        }
    }
}