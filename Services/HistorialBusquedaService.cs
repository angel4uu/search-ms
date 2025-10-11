using SearchMS.DTOs;
using SearchMS.Models;
using SearchMS.Repositories;
using SearchMS.Mappings;
using SearchMS.Interfaces;

namespace SearchMS.Services
{
    public class HistorialBusquedaService : IHistorialBusquedaService
    {
        private readonly IHistorialBusquedaRepository _repository;
        private readonly ILogger<HistorialBusquedaService> _logger;

        public HistorialBusquedaService(IHistorialBusquedaRepository repository, ILogger<HistorialBusquedaService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetHistorialByIdOutputDto?> GetByIdAsync(int id)
        {
            var historial = await _repository.GetByIdAsync(id);
            if (historial == null)
            {
                _logger.LogInformation("Historial not found with ID: {Id}", id);
                return null;
            }

            return historial.ToGetByIdDto();
        }

        public async Task<CreateHistorialOutputDto> CreateAsync(CreateHistorialInputDto input)
        {
            var historial = new HistorialBusqueda
            {
                TextoBusqueda = input.TextoBusqueda.Trim(),
                UsuarioId = input.UsuarioId.Trim(),
                FechaBusqueda = DateTime.UtcNow 
            };

            var created = await _repository.CreateAsync(historial);

            _logger.LogInformation(
                "Created historial {Id} for user {UsuarioId}", 
                created.Id, 
                created.UsuarioId);

            return created.ToCreateDto();
        }

        public async Task<GetHistorialByUsuarioIdOutputDto> GetByUsuarioIdAsync(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                throw new ArgumentException("UsuarioId cannot be empty", nameof(usuarioId));
            }

            var historiales = await _repository.GetByUsuarioIdAsync(usuarioId);

            return historiales.ToGetByUsuarioIdDto();
        }
    }
}