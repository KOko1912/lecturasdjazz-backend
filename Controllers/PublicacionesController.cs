using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LecturasJazz.API.Data;
using LecturasJazz.API.Models.Publicaciones;
using LecturasJazz.API.DTOs.Publicaciones;

namespace LecturasJazz.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicacionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PublicacionesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Crear(
            [FromForm] string descripcion,
            [FromForm] bool esVideo,
            [FromForm] int adminUserId,
            [FromForm] int? productoId,
            [FromForm] int? descuentoRelacionado,
            [FromForm] IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo inválido.");

            var extension = Path.GetExtension(archivo.FileName).ToLower();
            var permitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4" };
            if (!permitidas.Contains(extension))
                return BadRequest("Tipo de archivo no permitido.");

            var nombreLimpio = Slugify(descripcion);
            var nombreArchivo = $"{nombreLimpio}{extension}";
            var carpetaPublicaciones = Path.Combine(_env.WebRootPath, "publicaciones");

            if (!Directory.Exists(carpetaPublicaciones))
                Directory.CreateDirectory(carpetaPublicaciones);

            var rutaArchivo = Path.Combine(carpetaPublicaciones, nombreArchivo);

            using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var publicacion = new Publicacion
            {
                Descripcion = descripcion,
                EsVideo = esVideo,
                MediaUrl = $"/publicaciones/{nombreArchivo}",
                ProductoId = productoId,
                DescuentoRelacionado = descuentoRelacionado,
                AdminUserId = adminUserId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Publicaciones.Add(publicacion);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Publicación creada", publicacion.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var publicaciones = await _context.Publicaciones
                .Include(p => p.Producto)
                .Include(p => p.Comentarios).ThenInclude(c => c.Usuario)
                .Include(p => p.Likes)
                .Include(p => p.AdminUser)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            var resultado = publicaciones.Select(p => new PublicacionDto
            {
                Id = p.Id,
                AdminNombre = p.AdminUser?.Nombre ?? "Administrador",
                Descripcion = p.Descripcion,
                MediaUrl = p.MediaUrl,
                EsVideo = p.EsVideo,
                FechaCreacion = p.FechaCreacion,
                Comentarios = p.Comentarios.Select(c => new ComentarioDto
                {
                    Id = c.Id,
                    Comentario = c.Comentario,
                    Fecha = c.Fecha,
                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario?.Nombre ?? "Usuario",
                    FotoUsuarioUrl = !string.IsNullOrEmpty(c.Usuario?.FotoUrl) ? $"/{c.Usuario.FotoUrl.TrimStart('/')}" : ""
                }).ToList(),
                TotalLikes = p.Likes.Count,
                Likes = p.Likes.Select(l => new LikeDto
                {
                    UsuarioId = l.UsuarioId,
                    PublicacionId = l.PublicacionId
                }).ToList()
            });

            return Ok(resultado);
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> ToggleLike(int id, [FromBody] LikeDto dto)
        {
            var likeExistente = await _context.PublicacionLikes
                .FirstOrDefaultAsync(l => l.PublicacionId == id && l.UsuarioId == dto.UsuarioId);

            if (likeExistente != null)
                _context.PublicacionLikes.Remove(likeExistente);
            else
                _context.PublicacionLikes.Add(new PublicacionLike { PublicacionId = id, UsuarioId = dto.UsuarioId });

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}/comentario/{comentarioId}")]
        public async Task<IActionResult> EliminarComentario(int id, int comentarioId, [FromBody] EliminarComentarioDto dto)
        {
            var comentario = await _context.PublicacionComentarios
                .FirstOrDefaultAsync(c => c.Id == comentarioId && c.PublicacionId == id && c.UsuarioId == dto.UsuarioId);

            if (comentario == null)
                return NotFound("Comentario no encontrado o no autorizado");

            _context.PublicacionComentarios.Remove(comentario);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/comentario")]
        public async Task<IActionResult> Comentar(int id, [FromBody] ComentarioDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(dto.UsuarioId);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            var comentario = new PublicacionComentario
            {
                PublicacionId = id,
                UsuarioId = usuario.Id,
                Comentario = dto.Comentario
            };

            _context.PublicacionComentarios.Add(comentario);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPublicacion(int id)
        {
            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null)
                return NotFound("Publicación no encontrada");

            _context.Publicaciones.Remove(publicacion);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Publicación eliminada" });
        }

        private string Slugify(string texto)
        {
            var sinAcentos = new string(texto
                .ToLower()
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray());

            return System.Text.RegularExpressions.Regex.Replace(sinAcentos, @"[^a-z0-9]+", "-").Trim('-');
        }
    }
}
