using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Barberia.Api.Services
{
    public class QdrantService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public QdrantService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task GuardarVectorCitaAsync(
            string idVector,
            List<float> vector,
            object payload)
        {
            var qdrantUrl = _configuration["Qdrant:Url"];
            var apiKey = _configuration["Qdrant:ApiKey"];
            var collectionName = _configuration["Qdrant:CollectionName"];

            if (string.IsNullOrWhiteSpace(qdrantUrl))
                throw new InvalidOperationException("No se configuró la URL de Qdrant.");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("No se configuró la API Key de Qdrant.");

            if (string.IsNullOrWhiteSpace(collectionName))
                throw new InvalidOperationException("No se configuró el nombre de la colección.");

            var url = $"{qdrantUrl.TrimEnd('/')}/collections/{collectionName}/points?wait=true";

            var requestBody = new
            {
                points = new[]
                {
                    new
                    {
                        id = idVector,
                        vector,
                        payload
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Add("api-key", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al guardar vector en Qdrant: {responseContent}");
        }


        public async Task<List<QdrantSearchResult>> BuscarSimilaresAsync(
        List<float> vector,
        string clienteId,
        int limite = 5)
        {
            var qdrantUrl = _configuration["Qdrant:Url"];
            var apiKey = _configuration["Qdrant:ApiKey"];
            var collectionName = _configuration["Qdrant:CollectionName"];

            var url = $"{qdrantUrl!.TrimEnd('/')}/collections/{collectionName}/points/search";

            var requestBody = new
            {
                vector,
                limit = limite,
                with_payload = true,
                filter = new
                {
                    must = new[]
                    {
                new
                {
                    key = "clienteId",
                    match = new
                    {
                        value = clienteId
                    }
                }
            }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("api-key", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al buscar en Qdrant: {responseContent}");

            using var document = JsonDocument.Parse(responseContent);

            var resultados = new List<QdrantSearchResult>();

            foreach (var item in document.RootElement.GetProperty("result").EnumerateArray())
            {
                var payload = item.GetProperty("payload");

                resultados.Add(new QdrantSearchResult
                {
                    IdVector = item.GetProperty("id").GetString() ?? string.Empty,
                    Score = item.GetProperty("score").GetSingle(),
                    IdCita = payload.GetProperty("idCita").GetInt32(),
                    ClienteId = payload.GetProperty("clienteId").GetString() ?? string.Empty,
                    BarberoId = payload.GetProperty("barberoId").GetInt32(),
                    ServicioId = payload.GetProperty("servicioId").GetInt32(),
                    Estado = payload.GetProperty("estado").GetString() ?? string.Empty,
                    Observacion = payload.TryGetProperty("observacion", out var observacion)
                        ? observacion.GetString()
                        : null,
                    Texto = payload.TryGetProperty("texto", out var texto)
                        ? texto.GetString()
                        : null
                });
            }

            return resultados;
        }
        public class QdrantSearchResult
        {
            public string IdVector { get; set; } = string.Empty;
            public float Score { get; set; }
            public int IdCita { get; set; }
            public string ClienteId { get; set; } = string.Empty;
            public int BarberoId { get; set; }
            public int ServicioId { get; set; }
            public string Estado { get; set; } = string.Empty;
            public string? Observacion { get; set; }
            public string? Texto { get; set; }
        }
    }
}