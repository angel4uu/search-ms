using Azure.Search.Documents.Models;
using SearchMS.Models;
using SearchMS.Repositories; 
using SearchMS.DTOs;
using System.Linq; 
using System.Collections.Generic;
using SearchMS.Interfaces;
using Azure.Search.Documents;

namespace SearchMS.Services;

public class AISearchService : IAISearchService
{
    private readonly IAISearchRepository _repository;
    private const string SuggesterName = "sg"; 
    
    // Define the list of fields to retrieve once
    private static readonly string[] SelectFields = new[] { 
        "id", "nombre", "precio", "imagen", "tienePromocion", "calificacion" 
    };
    private static readonly string[] CollectionFields = new[] { "color", "talla" };


    public AISearchService(IAISearchRepository repository)
    {
        _repository = repository;
    }

    // --- Search with filtering, sorting, and pagination (IMPLEMENTATION) ---
    public async Task<PagedResponseDto<ProductResponseDto>> SearchAsync(SearchRequestDto request)
    {    
        var options = new SearchOptions();
        var filters = new List<string>();

        // Pagination
        int pageSize = Math.Clamp(request.PageSize, 1, 100); 
        int pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        int skip = (pageNumber - 1) * pageSize;

        // Select only necessary fields
        foreach (var field in SelectFields)
        {
            options.Select.Add(field); 
        }

        // Set pagination options
        options.Size = pageSize;
        options.Skip = skip;
        options.IncludeTotalCount = true; 

        // --- 1. Price and Promotion Filters ---
        if (request.PrecioMin.HasValue) filters.Add($"precio ge {request.PrecioMin.Value}");
        if (request.PrecioMax.HasValue) filters.Add($"precio le {request.PrecioMax.Value}");
        if (request.TienePromocion.HasValue) filters.Add($"tienePromocion eq {request.TienePromocion.Value.ToString().ToLower()}");

        // --- 2. Attribute Filters  ---
        // Simple String fields (use OR logic)
        AddAttributeFilter(filters, "categoria", request.Categoria);
        AddAttributeFilter(filters, "genero", request.Genero);
        AddAttributeFilter(filters, "deporte", request.Deporte);
        AddAttributeFilter(filters, "tipo", request.Tipo);
        AddAttributeFilter(filters, "coleccion", request.Coleccion);
        
        // Collection fields (use search.in logic)
        AddCollectionFieldFilter(filters, "color", request.Colores);
        AddCollectionFieldFilter(filters, "talla", request.Tallas);

        if (filters.Count > 0)
        {
            options.Filter = string.Join(" and ", filters);
        }

        // --- 3. Sorting ---
        if (!string.IsNullOrEmpty(request.OrderBy))
        {
            if (request.OrderBy.Equals("precio-asc", StringComparison.OrdinalIgnoreCase))
            {
                options.OrderBy.Add("precio asc");
            }
            else if (request.OrderBy.Equals("precio-desc", StringComparison.OrdinalIgnoreCase))
            {
                options.OrderBy.Add("precio desc");
            }
        }

        // Text Search
        string searchText = string.IsNullOrEmpty(request.SearchText) ? "*" : request.SearchText;

        // Execute Query via Repository
        SearchResults<ProductoIndexDocument> results = 
            await _repository.SearchAsync(searchText, options);

        // Build Paged Response and map to Presentation DTO
        var pagedResponse = new PagedResponseDto<ProductResponseDto>
        {
            CurrentPage = pageNumber,
            PageSize = pageSize,
            TotalCount = results.TotalCount ?? 0,
            Items = results.GetResults().Select(r => MapToResponseDto(r.Document)).ToList()
        };

        return pagedResponse;
    }

     // Autocomplete queries
    public async Task<List<string>> AutocompleteAsync(string searchText){

        if (string.IsNullOrEmpty(searchText)) return new List<string>();

        // Call the repository to get autocomplete results
        AutocompleteResults results = await _repository.AutocompleteAsync(searchText, SuggesterName);

        // Return just the text of the suggestions
        return results.Results.Select(r => r.Text).ToList();
    } 

    // Suggest documents
    public async Task<List<ProductResponseDto>> SuggestAsync(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
            return new List<ProductResponseDto>();
        
        var options = new SuggestOptions
        {
            Size = 5
        };
        
        // Populate Select fields for suggestions
        foreach (var field in SelectFields)
        {
            options.Select.Add(field);
        }

        SuggestResults<ProductoIndexDocument> results = 
            await _repository.SuggestAsync(searchText, SuggesterName, options);

        // Map suggest documents to Presentation DTO
        return results.Results.Select(r => MapToResponseDto(r.Document)).ToList();
    }

    // --- PRIVATE HELPER METHODS ---

     /// <summary>
    /// For simple string fields - use OR logic with individual comparisons
    /// </summary>
    private void AddAttributeFilter(List<string> filters, string fieldName, string[]? values)
    {
        if (values != null && values.Length > 0)
        {
            var sanitizedValues = values.Select(Sanitize).ToArray();
            
            // Use OR logic for simple String fields (categoria, genero, etc.)
            string orFilters = string.Join(" or ", sanitizedValues.Select(v => $"{fieldName} eq '{v}'"));
            filters.Add($"({orFilters})"); // Wrap in parentheses for precedence
        }
    }

    /// <summary>
    /// For collection fields - use search.in with proper formatting
    /// </summary>
    private void AddCollectionFieldFilter(List<string> filters, string fieldName, string[]? values)
    {
        if (values != null && values.Length > 0)
        {
            // For collection fields, we need to handle each value individually
            // since search.in requires single values
            var sanitizedValues = values.Select(Sanitize).ToArray();
            
            if (sanitizedValues.Length == 1)
            {
                // Single value - use simple equality
                filters.Add($"{fieldName}/any(t: t eq '{sanitizedValues[0]}')");
            }
            else
            {
                // Multiple values - use OR logic with collection field syntax
                var orConditions = sanitizedValues.Select(v => $"{fieldName}/any(t: t eq '{v}')");
                filters.Add($"({string.Join(" or ", orConditions)})");
            }
        }
    }

    /// <summary>
    /// Helper to sanitize filter values by escaping single quotes.
    /// </summary>
    private string Sanitize(string value) => value.Replace("'", "''");

    /// <summary>
    /// Helper method to map the internal index document to the external presentation DTO.
    /// </summary>
    private ProductResponseDto MapToResponseDto(ProductoIndexDocument doc)
    {
        return new ProductResponseDto
        {
            Id = doc.Id,
            Nombre = doc.Nombre,
            Precio = doc.Precio,
            Imagen = doc.Imagen,
            TienePromocion = doc.TienePromocion,
            Calificacion = doc.Calificacion
        };
    }
}