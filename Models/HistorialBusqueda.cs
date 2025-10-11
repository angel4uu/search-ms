using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SearchMS.Models
{
    [Table("historial_busqueda")]
    public class HistorialBusqueda
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("id_usuario")]
        [MaxLength(100)] 
        public string UsuarioId { get; set; } = string.Empty; 

        [Required]
        [Column("texto_busqueda")]
        [MaxLength(500)]
        public string TextoBusqueda { get; set; } = string.Empty;

        [Column("fecha_busqueda")]
        public DateTime FechaBusqueda { get; set; }
    }
}