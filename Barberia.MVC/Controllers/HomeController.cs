using Barberia.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using ModelosBarberia.ViewModels;
using ModelosBarberia;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json; // Asegura la lectura de Json
using System.Security.Claims;

namespace Barberia.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // 1. CORRECCIÓN: Declaramos el campo para el HttpClientFactory
        private readonly IHttpClientFactory _httpClientFactory;

        // 2. CORRECCIÓN: Lo agregamos al constructor para que el contenedor de dependencias lo inyecte
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var clienteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            var cliente = await client.GetFromJsonAsync<ApplicationUser>($"api/usuarios/{clienteId}");

            // Cargamos los servicios de la base de datos
            var servicios = await client.GetFromJsonAsync<List<Servicio>>("api/servicios") ?? new List<Servicio>();

            var citas = new List<Cita>();
            var citasResponse = await client.GetAsync($"api/citas/cliente/{clienteId}");
            if (citasResponse.IsSuccessStatusCode)
            {
                citas = await citasResponse.Content.ReadFromJsonAsync<List<Cita>>() ?? new List<Cita>();
            }

            var vm = new ClienteDashboardViewModel
            {
                Cliente = cliente!,
                HistorialCitas = citas,
                // 3. CORRECCIÓN: Eliminamos el espacio en blanco y usamos un nombre válido
                // Nota: Si en tu ClienteDashboardViewModel la propiedad se llama distinto, cámbiala aquí
                Servicios = servicios.Where(s => s.Activo).ToList(),
                CitasPendientes = citas.Count(c => c.Estado == ModelosBarberia.Enum.EstadoCita.Pendiente),
                CitasCompletadas = citas.Count(c => c.Estado == ModelosBarberia.Enum.EstadoCita.Atendida),
                ProximaCita = citas
                    .Where(c => c.FechaHora >= DateTime.Now && c.Estado != ModelosBarberia.Enum.EstadoCita.Cancelada)
                    .OrderBy(c => c.FechaHora)
                    .FirstOrDefault()
            };

            return View(vm);
        }

        public IActionResult QuienesSomos()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}