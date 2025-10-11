namespace SearchMS.DTOs
{
  public class ConnectionTestOutputDto
  {
      public bool IsConnected { get; set; }
      public string Message { get; set; } = string.Empty;
      public DateTime Timestamp { get; set; }
      public string? ErrorDetails { get; set; }
  }
  public class SearchProductInputDto { }
  public class SearchProductOutputDto { }
  public class FilterProductInputDto { }
  public class FilterProductOutputDto { }
  public class SuggestProductInputDto { }
  public class SuggestProductOutputDto { }
  public class ProductDto { }
}