using System.Text.Json.Serialization;

namespace SearchMS.Models;

/// <summary>
/// Defines the complete structure of a document in the Azure AI Search index.
/// </summary>
public partial class ProductoIndexDocument
{
    // --- Core Retrievable Fields ---
    
    [JsonPropertyName("id")] 
    public string Id { get; set; }
    
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }

    [JsonPropertyName("precio")]
    public double? Precio { get; set; }

    [JsonPropertyName("imagen")]
    public string Imagen { get; set; }

    [JsonPropertyName("tienePromocion")]
    public bool? TienePromocion { get; set; }

    [JsonPropertyName("calificacion")] // Note: No accent to match the index field name
    public double? Calificacion { get; set; }
    
    // --- Filterable (Non-retrievable/Retrievable for Debug) Fields ---
    
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; } // Included because it's used by the Scoring Profile

    [JsonPropertyName("categoria")]
    public string? Categoria { get; set; }

    [JsonPropertyName("genero")]
    public string? Genero { get; set; }

    [JsonPropertyName("deporte")]
    public string? Deporte { get; set; }

    [JsonPropertyName("tipo")]
    public string? Tipo { get; set; }

    [JsonPropertyName("coleccion")]
    public string? Coleccion { get; set; }

    [JsonPropertyName("color")]
    public string[]? Color { get; set; }

    [JsonPropertyName("talla")]
    public string[]? Talla { get; set; }
}