using Microsoft.EntityFrameworkCore;
using SearchMS.Data;
using SearchMS.Models;
using SearchMS.Interfaces;

namespace SearchMS.Repositories
{
    public class HistorialBusquedaRepository : IHistorialBusquedaRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HistorialBusquedaRepository> _logger;

        public HistorialBusquedaRepository(ApplicationDbContext context, ILogger<HistorialBusquedaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HistorialBusqueda?> GetByIdAsync(int id)
        {
            return await _context.HistorialBusquedas
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<HistorialBusqueda> CreateAsync(HistorialBusqueda historial)
        {
            try
            {
                _context.HistorialBusquedas.Add(historial);
                await _context.SaveChangesAsync();
                return historial;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating historial for user {UsuarioId}", 
                    historial.UsuarioId);
                throw;
            }
        }

        public async Task<List<HistorialBusqueda>> GetByUsuarioIdAsync(string usuarioId)
        {
            return await _context.HistorialBusquedas
                .AsNoTracking()
                .Where(h => h.UsuarioId == usuarioId)
                .OrderByDescending(h => h.FechaBusqueda)
                .ToListAsync();
        }

    }
}