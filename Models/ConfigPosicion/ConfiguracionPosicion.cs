using System.ComponentModel.DataAnnotations;

namespace LecturasJazz.API.Models.ConfigPosicion
{
    public class ConfiguracionPosicion
    {
        public int Id { get; set; }

        [Required]
        public string Tipo { get; set; } = null!;  // "producto" o "categoria"

        public Guid? Uuid { get; set; }  // Para producto
        public string? Categoria { get; set; }  // Para categoría

        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }
}
