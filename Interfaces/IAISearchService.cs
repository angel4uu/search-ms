using SearchMS.DTOs;

namespace SearchMS.Interfaces
{
    public interface IAISearchService
    {
        Task<ConnectionTestOutputDto> TestConnectionAsync();
        Task<SearchProductOutputDto> SearchProductsAsync(SearchProductInputDto input);
        Task<FilterProductOutputDto> FilterProductsAsync(FilterProductInputDto input);
        Task<SuggestProductOutputDto> SuggestProductsAsync(SuggestProductInputDto input);
    }
}