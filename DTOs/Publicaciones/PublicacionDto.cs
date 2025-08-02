using System;
using System.Collections.Generic;

namespace LecturasJazz.API.DTOs.Publicaciones
{
    public class PublicacionDto
    {
        public int Id { get; set; }
        public required string AdminNombre { get; set; }
        public required string Descripcion { get; set; }
        public required string MediaUrl { get; set; }
        public bool EsVideo { get; set; }
        public DateTime FechaCreacion { get; set; }

        public ProductoMiniDto? Producto { get; set; }

        public List<ComentarioDto> Comentarios { get; set; } = new();
        public int TotalLikes { get; set; }
        public bool UsuarioYaDioLike { get; set; }

        public List<LikeDto> Likes { get; set; } = new();
    }

    public class ProductoMiniDto
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public required string ImagenUrl { get; set; }
        public decimal Precio { get; set; }
        public bool EnOferta { get; set; }
        public int? PorcentajeOferta { get; set; }
    }
}
