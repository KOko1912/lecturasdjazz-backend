using System;
using System.ComponentModel.DataAnnotations;

namespace LecturasJazz.API.Models.Admin
{
    public class Producto
    {
        public int Id { get; set; }

        public Guid Uuid { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "El título es obligatorio.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
        public decimal Precio { get; set; }

        public bool EnOferta { get; set; } = false;

        [Range(0, 100, ErrorMessage = "El porcentaje de oferta debe estar entre 0 y 100.")]
        public int PorcentajeOferta { get; set; } = 0;

        // Este campo no se guarda en la base de datos, es solo para cálculo en frontend o respuesta
        public decimal PrecioFinal => EnOferta ? Precio * (1 - PorcentajeOferta / 100m) : Precio;

        public string ImagenUrl { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser un número positivo.")]
        public int Stock { get; set; } = 0;

        public bool Activo { get; set; } = true;

        public string Categoria { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        public bool Destacado { get; set; } = false;

        public int ConteoVisitas { get; set; } = 0;
    }
}
