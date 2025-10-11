using SearchMS.DTOs;
using SearchMS.Settings;
using SearchMS.Interfaces;
using Microsoft.Extensions.Options;

namespace SearchMS.Clients
{
    public class AISearchClient : IAISearchClient
    {
        private readonly IOptions<AISearchSettings> _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AISearchClient> _logger;
        private const string ApiVersion = "2023-11-01";

        public AISearchClient(IOptions<AISearchSettings> settings, HttpClient httpClient, ILogger<AISearchClient> logger)
        {
            _settings = settings;
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<ConnectionTestOutputDto> TestConnectionAsync()
        {
            var result = new ConnectionTestOutputDto
            {
                Timestamp = DateTime.UtcNow
            };

            try
            {
                // Validate settings first
                if (string.IsNullOrEmpty(_settings.Value.Endpoint))
                {
                    _logger.LogWarning("AI Search endpoint is not configured");
                    result.IsConnected = false;
                    result.Message = "❌ AI Search endpoint is not configured";
                    result.ErrorDetails = "Missing endpoint in configuration";
                    return result;
                }

                if (string.IsNullOrEmpty(_settings.Value.ApiKey))
                {
                    _logger.LogWarning("AI Search API key is not configured");
                    result.IsConnected = false;
                    result.Message = "❌ AI Search API key is not configured";
                    result.ErrorDetails = "Missing API key in configuration";
                    return result;
                }

                // Test connection by listing indexes
                var testUrl = $"/indexes?api-version={ApiVersion}";
                var request = new HttpRequestMessage(HttpMethod.Get, testUrl);
                request.Headers.Add("api-key", _settings.Value.ApiKey);
                
                _logger.LogInformation("Testing connection to Azure AI Search at {Endpoint}", _settings.Value.Endpoint);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    result.IsConnected = true;
                    result.Message = "✅ Successfully connected to Azure AI Search";
                    _logger.LogInformation("Azure AI Search connection test succeeded");
                }
                else
                {
                    var statusCode = (int)response.StatusCode;
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    result.IsConnected = false;
                    result.Message = $"❌ Connection failed with status {statusCode}";
                    result.ErrorDetails = errorContent;
                    
                    _logger.LogWarning(
                        "Azure AI Search connection test failed with status {StatusCode}: {Error}",
                        statusCode,
                        errorContent);
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                result.IsConnected = false;
                result.Message = "❌ Network error connecting to Azure AI Search";
                result.ErrorDetails = ex.Message;
                _logger.LogError(ex, "Network error testing Azure AI Search connection");
                return result;
            }
            catch (Exception ex)
            {
                result.IsConnected = false;
                result.Message = "❌ Unexpected error testing connection";
                result.ErrorDetails = ex.Message;
                _logger.LogError(ex, "Unexpected error testing Azure AI Search connection");
                return result;
            }
        }
        public async Task<SearchProductOutputDto> SearchAsync(SearchProductInputDto input)
        {
          throw new NotImplementedException();
        }
        public async Task<FilterProductOutputDto> FilterAsync(FilterProductInputDto input)
        {
          throw new NotImplementedException();
        }
        public async Task<SuggestProductOutputDto> SuggestAsync(SuggestProductInputDto input)
        {
          throw new NotImplementedException();
        }
    }
}