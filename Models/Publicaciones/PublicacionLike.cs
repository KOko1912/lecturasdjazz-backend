using System;

namespace LecturasJazz.API.Models.Publicaciones
{
    public class PublicacionLike
    {
        public int Id { get; set; }
        public int PublicacionId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // ✅ Nullable: serán seteadas por EF
        public Publicacion? Publicacion { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
