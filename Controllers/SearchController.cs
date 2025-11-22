using Microsoft.AspNetCore.Mvc;
using SearchMS.Interfaces;
using SearchMS.DTOs;

namespace SearchMS.Controllers;

[ApiController]
[Route("api/search")] 
public class SearchController : ControllerBase
{
    private readonly IAISearchService _searchService;

    public SearchController(IAISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Performs a product search using all available filters and pagination.
    /// This endpoint uses POST to accept a complex JSON request body.
    /// </summary>
    [HttpPost] 
    public async Task<IActionResult> Search([FromBody] SearchRequestDto request)
    {
        try
        {
            var results = await _searchService.SearchAsync(request);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets autocomplete suggestions for a partial query.
    /// e.g., /api/search/autocomplete?q=puma
    /// </summary>
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string q)
    {
        if (string.IsNullOrEmpty(q))
            return BadRequest("Query parameter 'q' is required.");
        
        try
        {
            var suggestions = await _searchService.AutocompleteAsync(q);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets document suggestions for a query.
    /// e.g., /api/search/suggest?q=reebok
    /// </summary>
    [HttpGet("suggest")] 
    public async Task<IActionResult> Suggest([FromQuery] string q)
    {
        if (string.IsNullOrEmpty(q))
            return BadRequest("Query parameter 'q' is required.");

        try
        {
            var suggestions = await _searchService.SuggestAsync(q);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}