using Barberia.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using ModelosBarberia;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;

namespace Barberia.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("BarberiaApi");

            // Al ser público, solo nos interesa cargar la lista de servicios para mostrar el catálogo
            var servicios = new List<Servicio>();

            try
            {
                var response = await client.GetFromJsonAsync<List<Servicio>>("api/servicios");
                if (response != null)
                {
                    servicios = response.Where(s => s.Activo).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al conectar con la API de servicios: {ex.Message}");
                // Mantenemos la lista vacía para evitar que la página se caiga si la API no responde
            }

            // Enviamos directamente la lista de servicios a la vista pública
            return View(servicios);
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