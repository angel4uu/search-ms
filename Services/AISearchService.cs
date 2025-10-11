using SearchMS.DTOs;
using SearchMS.Interfaces;

namespace SearchMS.Services{
  public class AISearchService : IAISearchService
    {
        private readonly IAISearchClient _searchClient;
        private readonly ILogger<AISearchService> _logger;

        public AISearchService(IAISearchClient searchClient, ILogger<AISearchService> logger)
        {
            _searchClient = searchClient;
            _logger = logger;
        }
        public async Task<ConnectionTestOutputDto> TestConnectionAsync()
        {
            _logger.LogInformation("Testing Azure AI Search connection");
            return await _searchClient.TestConnectionAsync();
        }
        public async Task<SearchProductOutputDto> SearchProductsAsync(SearchProductInputDto input)
        {
          throw new NotImplementedException();
        }
        public async Task<FilterProductOutputDto> FilterProductsAsync(FilterProductInputDto input)
        {
          throw new NotImplementedException();
        }
        public async Task<SuggestProductOutputDto> SuggestProductsAsync(SuggestProductInputDto input)
        {
          throw new NotImplementedException();
        }
    }
}