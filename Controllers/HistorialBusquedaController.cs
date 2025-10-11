using Microsoft.AspNetCore.Mvc;
using SearchMS.DTOs;
using SearchMS.Services;
using SearchMS.Interfaces;

namespace SearchMS.Controllers
{
    [ApiController]
    [Route("api/historial")] 
    [Produces("application/json")]
    public class HistorialBusquedaController : ControllerBase
    {
        private readonly IHistorialBusquedaService _historialService;
        private readonly ILogger<HistorialBusquedaController> _logger;

        public HistorialBusquedaController(
            IHistorialBusquedaService historialService, 
            ILogger<HistorialBusquedaController> logger)
        {
            _historialService = historialService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a specific search history entry by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetHistorialByIdOutputDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetHistorialByIdOutputDto>> GetById(int id)
        {   
            if (id <= 0)
            {
                return BadRequest(new { error = "ID must be greater than 0" });
            }
            try
            {
                var result = await _historialService.GetByIdAsync(id);
                
                if (result == null)
                {
                    return NotFound(new { error = $"Historial with ID {id} not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting historial by ID: {Id}", id);
                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        /// <summary>
        /// Creates a new search history entry
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CreateHistorialOutputDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateHistorialOutputDto>> Create([FromBody] CreateHistorialInputDto input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _historialService.CreateAsync(input);
                return CreatedAtAction(
                    nameof(GetById), 
                    new { id = result.Id }, 
                    result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for creating historial");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating historial for user {UsuarioId}", input.UsuarioId);
                return StatusCode(500, new { error = "An error occurred while creating the historial" });
            }
        }

        /// <summary>
        /// Gets search history for a specific user 
        /// </summary>
        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(GetHistorialByUsuarioIdOutputDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetHistorialByUsuarioIdOutputDto>> GetByUsuarioId(string usuarioId)
        {   
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return BadRequest(new { error = "UsuarioId cannot be empty" });
            }
            try
            {
                var result = await _historialService.GetByUsuarioIdAsync(usuarioId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for getting historial by usuarioId");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting historial by usuarioId: {UsuarioId}", usuarioId);
                return StatusCode(500, new { error = "An error occurred while retrieving the historial" });
            }
        }
    }
}