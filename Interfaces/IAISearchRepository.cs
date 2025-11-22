using SearchMS.Models;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace SearchMS.Interfaces
{
  public interface IAISearchRepository
  {
      // Executes a search query.
      Task<SearchResults<ProductoIndexDocument>> SearchAsync(string searchText, SearchOptions options);

      // Executes an autocomplete query.
      Task<AutocompleteResults> AutocompleteAsync(string searchText, string suggesterName);

      // Executes a suggest query.
      Task<SuggestResults<ProductoIndexDocument>> SuggestAsync(string searchText, string suggesterName, SuggestOptions options);
  }
}