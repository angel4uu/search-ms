using SearchMS.DTOs;

namespace SearchMS.Interfaces
{

    public interface IAISearchService
    {
        // Search with filtering, sorting, and pagination
        Task<PagedResponseDto<ProductResponseDto>> SearchAsync(SearchRequestDto request);
        // Autocomplete queries
        Task<List<string>> AutocompleteAsync(string searchText);
        // Suggest documents
        Task<List<ProductResponseDto>> SuggestAsync(string searchText);
    }
}