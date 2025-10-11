using SearchMS.DTOs;

namespace SearchMS.Interfaces
{
    public interface IAISearchClient
    {
        Task<ConnectionTestOutputDto> TestConnectionAsync();
        Task<SearchProductOutputDto> SearchAsync(SearchProductInputDto input);
        Task<FilterProductOutputDto> FilterAsync(FilterProductInputDto input);
        Task<SuggestProductOutputDto> SuggestAsync(SuggestProductInputDto input);
    }
}