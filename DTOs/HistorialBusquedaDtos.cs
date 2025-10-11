using System.ComponentModel.DataAnnotations;

namespace SearchMS.DTOs
{
    public class CreateHistorialInputDto
    {
        [Required(ErrorMessage = "El texto de búsqueda es requerido")]
        [MaxLength(500, ErrorMessage = "El texto de búsqueda no puede exceder 500 caracteres")]
        [MinLength(1, ErrorMessage = "El texto de búsqueda no puede estar vacío")]
        public string TextoBusqueda { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID de usuario es requerido")]
        [MaxLength(100)]
        public string UsuarioId { get; set; } = string.Empty;
    }

    public class CreateHistorialOutputDto
    {
        public int Id { get; set; }
        public string TextoBusqueda { get; set; } = string.Empty;
        public DateTime FechaBusqueda { get; set; }
    }

    public class GetHistorialByIdOutputDto
    {
        public int Id { get; set; }
        public string TextoBusqueda { get; set; } = string.Empty;
        public DateTime FechaBusqueda { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
    }

    public class GetHistorialByUsuarioIdOutputDto
    {
        public List<HistorialItemDto> Historiales { get; set; } = new();
    }

    public class HistorialItemDto
    {
        public int Id { get; set; }
        public string TextoBusqueda { get; set; } = string.Empty;
        public DateTime FechaBusqueda { get; set; }
    }
}