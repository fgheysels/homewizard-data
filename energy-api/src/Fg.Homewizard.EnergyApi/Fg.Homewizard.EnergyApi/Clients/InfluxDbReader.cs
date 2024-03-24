using Fg.Homewizard.EnergyApi.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Fg.Homewizard.EnergyApi.Clients
{
    public class InfluxDbReader
    {
        private readonly HttpClient _httpClient;
        private readonly InfluxDbSettings _settings;
        private readonly ILogger<InfluxDbReader> _logger;

        private readonly static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public InfluxDbReader(HttpClient http, IOptions<InfluxDbSettings> settings, ILogger<InfluxDbReader> logger)
        {
            _httpClient = http;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<InfluxQlResults> QueryQLAsync(string query)
        {
            var formContent = new MultipartFormDataContent();

            formContent.Add(new StringContent(_settings.DatabaseName), "db");
            formContent.Add(new StringContent(query), "q");

            var response = await _httpClient.PostAsync($"{_settings.InfluxDbUrl}/query", formContent);

            response.EnsureSuccessStatusCode();

            var rawContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<InfluxQlResults>(rawContent, SerializerOptions);
        }
    }
}
