using System;

namespace LecturasJazz.API.Models.Publicaciones
{
    public class PublicacionComentario
    {
        public int Id { get; set; }
        public int PublicacionId { get; set; }
        public int UsuarioId { get; set; }

        public required string Comentario { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public Publicacion? Publicacion { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
