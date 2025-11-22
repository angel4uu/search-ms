namespace SearchMS.DTOs
{
  public class SearchRequestDto
  {
      public string? SearchText { get; set; }
      public int PageNumber { get; set; } = 1;
      public int PageSize { get; set; } = 20;
      //Valid values: "precio asc", "precio desc"
      public string? OrderBy { get; set; }
      public double? PrecioMin { get; set; }
      public double? PrecioMax { get; set; }
      public bool? TienePromocion { get; set; }
      public string[]? Categoria { get; set; }
      public string[]? Genero { get; set; }
      public string[]? Deporte { get; set; }
      public string[]? Tipo { get; set; }
      public string[]? Coleccion { get; set; }
      public string[]? Colores { get; set; }
      public string[]? Tallas { get; set; }
  }
  public class PagedResponseDto<T>
  {
      public List<T> Items { get; set; } = new();
      public int CurrentPage { get; set; } = 1;
      public int PageSize { get; set; } = 20;
      public long TotalCount { get; set; } = 0;

      public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
  }
  public class ProductResponseDto
{
    public string? Id { get; set; }
    public string? Nombre { get; set; }
    public double? Precio { get; set; }
    public string? Imagen { get; set; }
    public bool? TienePromocion { get; set; }
    public double? Calificacion { get; set; }
}
}