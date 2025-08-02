using LecturasJazz.API.Models;
using LecturasJazz.API.Models.Admin;
using LecturasJazz.API.Models.Reacciones;
using Microsoft.EntityFrameworkCore;
using LecturasJazz.API.Models.Publicaciones;

namespace LecturasJazz.API.Data // <-- CORREGIDO AQUÍ
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<AdminUser> AdminUsers { get; set; } = null!;
        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<Comentario> Comentarios { get; set; } = null!;
        public DbSet<Calificacion> Calificaciones { get; set; } = null!;
        public DbSet<MeGusta> MeGustas { get; set; } = null!;
        public DbSet<ConfiguracionServicios> ConfiguracionServicios { get; set; } = null!;
        public DbSet<LecturasJazz.API.Models.ConfigPosicion.ConfiguracionPosicion> ConfiguracionPosicion { get; set; }

        public DbSet<Publicacion> Publicaciones { get; set; }
        public DbSet<PublicacionComentario> PublicacionComentarios { get; set; }
        public DbSet<PublicacionLike> PublicacionLikes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.Property(p => p.Precio).HasPrecision(18, 2);
                entity.HasIndex(p => p.Uuid).IsUnique();
            });

            // Like único por usuario/publicación
            modelBuilder.Entity<PublicacionLike>()
             .HasIndex(l => new { l.PublicacionId, l.UsuarioId })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
