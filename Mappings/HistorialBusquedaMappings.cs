using SearchMS.DTOs;
using SearchMS.Models;

namespace SearchMS.Mappings
{
    public static class HistorialBusquedaMappings
    {
        public static GetHistorialByIdOutputDto ToGetByIdDto(this HistorialBusqueda historial) => new()
        {
            Id = historial.Id,
            TextoBusqueda = historial.TextoBusqueda,
            FechaBusqueda = historial.FechaBusqueda,
            UsuarioId = historial.UsuarioId
        };

        public static CreateHistorialOutputDto ToCreateDto(this HistorialBusqueda historial) => new()
        {
            Id = historial.Id,
            TextoBusqueda = historial.TextoBusqueda,
            FechaBusqueda = historial.FechaBusqueda
        };

        public static HistorialItemDto ToItemDto(this HistorialBusqueda historial) => new()
        {
            Id = historial.Id,
            TextoBusqueda = historial.TextoBusqueda,
            FechaBusqueda = historial.FechaBusqueda
        };

        public static GetHistorialByUsuarioIdOutputDto ToGetByUsuarioIdDto(this List<HistorialBusqueda> historiales) => new()
        {
            Historiales = historiales.Select(h => h.ToItemDto()).ToList()
        };
    }
}