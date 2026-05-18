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

            // 1. SOLUCIÓN AL ID: 
            // Si tu tabla 'Barberos' tiene un Id numérico, pero Identity usa String GUID, 
            // necesitas obtener el Id numérico del barbero usando su Email o su UserId de la API.
            // Por ahora, asumiremos que logras obtener el entero, o puedes hacer una petición intermedia:

            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // Suponiendo que buscas el barbero asociado al usuario logueado:
            var barberos = await client.GetFromJsonAsync<List<Barbero>>("api/barberos") ?? new();
            var barberoActual = barberos.FirstOrDefault(b => b.Email == userEmail);

            if (barberoActual == null)
            {
                TempData["Error"] = "No se encontró el perfil de barbero asociado a esta cuenta.";
                return RedirectToAction("Index", "Home");
            }

            int barberoId = barberoActual.Id; // Aquí tienes el int real de la base de datos

            // 2. SOLUCIÓN AL MApeo: Recibimos CitaBarberoDto como lo manda tu API
            var citasDto = await client.GetFromJsonAsync<List<CitaBarberoDto>>($"api/citas/barbero/{barberoId}") ?? new();

            // Si tu 'BarberoDashboardViewModel' requiere estrictamente List<Cita>, 
            // adaptamos los datos del DTO a Citas ficticias para que la vista no se rompa:
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

            var dashboard = new BarberoDashboardViewModel
            {
                Citas = citasMapeadas, // Ahora la lista coincide perfectamente
                CitasHoy = citasMapeadas.Count(c => c.FechaHora.Date == DateTime.Today),
                CitasPendientes = citasMapeadas.Count(c => c.Estado == EstadoCita.Pendiente),
                CitasCompletadas = citasMapeadas.Count(c => c.Estado == EstadoCita.Atendida),
                TopClientes = new List<ApplicationUser>()
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