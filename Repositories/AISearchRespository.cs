using SearchMS.Interfaces;
using SearchMS.Models;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using SearchMS.Settings;
using Microsoft.Extensions.Options;

namespace SearchMS.Repositories
{ 
    public class AISearchRepository : IAISearchRepository
    {
        private readonly SearchClient _searchClient;
    
        public AISearchRepository(IOptions<AISearchSettings> settings)
        {
            var config = settings.Value;

            if (string.IsNullOrEmpty(config.ServiceEndpoint))
                throw new ArgumentNullException(nameof(config.ServiceEndpoint));
            if (string.IsNullOrEmpty(config.IndexName))
                throw new ArgumentNullException(nameof(config.IndexName));
            if (string.IsNullOrEmpty(config.ApiKey))
                throw new ArgumentNullException(nameof(config.ApiKey));

            _searchClient = new SearchClient(
                new Uri(config.ServiceEndpoint),
                config.IndexName,
                new AzureKeyCredential(config.ApiKey));
        }

        // Executes a search query with the given options.
        public async Task<SearchResults<ProductoIndexDocument>> SearchAsync(string searchText, SearchOptions options)
        {
            return await _searchClient.SearchAsync<ProductoIndexDocument>(searchText, options);
        }

        // Executes an autocomplete query using the specified suggester.
        public async Task<AutocompleteResults> AutocompleteAsync(string searchText, string suggesterName)
        {
            var options = new AutocompleteOptions
            {
                UseFuzzyMatching = true, // Allows for spelling errors
                Size = 5 // Get top 5 completions
            };
            return await _searchClient.AutocompleteAsync(searchText, suggesterName, options);
        }

        // Executes a suggest query using the specified suggester.
        public async Task<SuggestResults<ProductoIndexDocument>> SuggestAsync(string searchText, string suggesterName, SuggestOptions? options = null)
        {
            return await _searchClient.SuggestAsync<ProductoIndexDocument>(searchText, suggesterName, options);
        }
    }
}