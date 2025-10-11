using SearchMS.DTOs;

namespace SearchMS.Interfaces
{
    public interface IHistorialBusquedaService
    {
        Task<GetHistorialByIdOutputDto?> GetByIdAsync(int id);
        Task<CreateHistorialOutputDto> CreateAsync(CreateHistorialInputDto input);
        Task<GetHistorialByUsuarioIdOutputDto> GetByUsuarioIdAsync(string usuarioId);
    }
}