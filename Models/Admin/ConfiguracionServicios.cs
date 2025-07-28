using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LecturasJazz.API.Models.Admin
{
    public class ConfiguracionServicios
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        public string? Subtitulo { get; set; }

        // Lista de UUIDs de productos, almacenados como texto separado por comas
        [Required]
        public string ProductosUuids { get; set; } = string.Empty;

        public int Orden { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public int PosX { get; set; } = 0;
        public int PosY { get; set; } = 0;
        public int Ancho { get; set; } = 300;
        public int Alto { get; set; } = 200;

    }
}
