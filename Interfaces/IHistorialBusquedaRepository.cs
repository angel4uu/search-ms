using SearchMS.Models;

namespace SearchMS.Interfaces
{
    public interface IHistorialBusquedaRepository
    {
        Task<HistorialBusqueda?> GetByIdAsync(int id);
        Task<HistorialBusqueda> CreateAsync(HistorialBusqueda historial);
        Task<List<HistorialBusqueda>> GetByUsuarioIdAsync(string usuarioId);
    }
}