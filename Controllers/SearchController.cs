using Microsoft.AspNetCore.Mvc;
using SearchMS.DTOs;
using SearchMS.Services;
using SearchMS.Interfaces;

namespace SearchMS.Controllers
{
    [ApiController]
    [Route("api")]
    public class AISearchController : ControllerBase
    {
        private readonly IAISearchService _searchService;
        private readonly ILogger<AISearchController> _logger;

        public AISearchController(IAISearchService searchService, ILogger<AISearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        /// <summary>
        /// Tests the connection to Azure AI Search
        /// </summary>
        [HttpGet("test-connection")]
        [ProducesResponseType(typeof(ConnectionTestOutputDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ConnectionTestOutputDto>> TestConnection()
        {
            try
            {
                var result = await _searchService.TestConnectionAsync();
                
                // Return 200 OK even if connection failed - the result object contains the status
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in TestConnection endpoint");
                
                return Ok(new ConnectionTestOutputDto
                {
                    IsConnected = false,
                    Message = "❌ Unexpected error occurred",
                    Timestamp = DateTime.UtcNow,
                    ErrorDetails = ex.Message
                });
            }
        }

        // [HttpPost("search")]
        // public async Task<ActionResult<SearchProductOutputDto>> Search([FromBody] SearchProductInputDto input)
        // {
        // }

        // [HttpPost("filter")]
        // public async Task<ActionResult<FilterProductOutputDto>> Filter([FromBody] FilterProductInputDto input)
        // {
        // }

        // [HttpPost("suggest")]
        // public async Task<ActionResult<SuggestProductOutputDto>> Suggest([FromBody] SuggestProductInputDto input)
        // {
        // }
    }
}