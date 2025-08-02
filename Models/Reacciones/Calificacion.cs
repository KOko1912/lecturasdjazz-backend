using System;
using System.ComponentModel.DataAnnotations;

namespace LecturasJazz.API.Models.Reacciones
{
    public class Calificacion
    {
        public int Id { get; set; }

        [Required]
        public string ProductoUuid { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Valor { get; set; }

        public int UsuarioId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
