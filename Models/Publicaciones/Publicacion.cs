using System;
using System.Collections.Generic;
using LecturasJazz.API.Models; // ✅ Para que encuentre Producto
using LecturasJazz.API.Models.Admin;

namespace LecturasJazz.API.Models.Publicaciones
{
    public class Publicacion
    {
        public int Id { get; set; }
        public int AdminUserId { get; set; }

        public required string Descripcion { get; set; }
        public required string MediaUrl { get; set; }

        public bool EsVideo { get; set; }
        public int? ProductoId { get; set; }
        public int? DescuentoRelacionado { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public AdminUser? AdminUser { get; set; }
        public Producto? Producto { get; set; }

        public List<PublicacionComentario> Comentarios { get; set; } = new();
        public List<PublicacionLike> Likes { get; set; } = new();
    }
}
