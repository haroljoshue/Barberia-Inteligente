using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Barberia.Api.Services
{
    public class EmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public EmbeddingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<float>> GenerarEmbeddingAsync(string texto)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var modelo = _configuration["OpenAI:EmbeddingModel"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("No se configuró la API Key de OpenAI.");

            if (string.IsNullOrWhiteSpace(modelo))
                throw new InvalidOperationException("No se configuró el modelo de embeddings.");

            var requestBody = new
            {
                model = modelo,
                input = texto
            };

            var json = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/embeddings");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al generar embedding: {responseContent}");

            using var document = JsonDocument.Parse(responseContent);

            var embedding = document.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToList();

            return embedding;
        }
    }
}
