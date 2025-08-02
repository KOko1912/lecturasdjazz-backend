using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LecturasJazz.API.DTOs.Racciones;
using LecturasJazz.API.Models.Reacciones;
using LecturasJazz.API.Data;

namespace LecturasJazz.API.Controllers.Usuarios
{
    [ApiController]
    [Route("api/productos/{uuid}/[controller]")]
    public class ReaccionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReaccionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ⭐ Calificar producto
        [HttpPost("calificacion")]
        public async Task<IActionResult> Calificar(string uuid, [FromBody] CalificacionDTO dto)
        {
            var existente = await _context.Calificaciones
                .FirstOrDefaultAsync(c => c.ProductoUuid == uuid && c.UsuarioId == dto.UsuarioId);

            if (existente != null)
            {
                existente.Valor = dto.Valor;
                existente.Fecha = DateTime.UtcNow;
            }
            else
            {
                _context.Calificaciones.Add(new Calificacion
                {
                    ProductoUuid = uuid,
                    UsuarioId = dto.UsuarioId,
                    Valor = dto.Valor,
                    Fecha = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Calificación registrada o actualizada" });
        }

        // ❤️ Like / Quitar like
        [HttpPost("like")]
        public async Task<IActionResult> ToggleLike(string uuid, [FromBody] MeGustaDTO dto)
        {
            var existente = await _context.MeGustas
                .FirstOrDefaultAsync(m => m.ProductoUuid == uuid && m.UsuarioId == dto.UsuarioId);

            if (dto.Accion == "agregar")
            {
                if (existente == null)
                {
                    _context.MeGustas.Add(new MeGusta
                    {
                        ProductoUuid = uuid,
                        UsuarioId = dto.UsuarioId,
                        Fecha = DateTime.UtcNow // ✅ aseguramos que no sea NULL
                    });
                    await _context.SaveChangesAsync();
                }
            }
            else if (dto.Accion == "quitar")
            {
                if (existente != null)
                {
                    _context.MeGustas.Remove(existente);
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { mensaje = "Acción registrada" });
        }

        // 💬 Guardar comentario
        [HttpPost("comentarios")]
        public async Task<IActionResult> Comentar(string uuid, [FromBody] ComentarioDTO dto)
        {
            _context.Comentarios.Add(new Comentario
            {
                ProductoUuid = uuid,
                Texto = dto.Texto,
                UsuarioId = dto.UsuarioId,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Comentario guardado" });
        }

        // 📖 Obtener comentarios
      [HttpGet("comentarios")]
public async Task<IActionResult> ObtenerComentarios(string uuid)
{
    var lista = await _context.Comentarios
        .Where(c => c.ProductoUuid == uuid)
        .OrderByDescending(c => c.Fecha)
        .Join(
            _context.Usuarios,
            c => c.UsuarioId,
            u => u.Id,
            (c, u) => new
            {
                c.Id,
                c.Texto,
                c.Fecha,
                c.UsuarioId,
                UsuarioNombre = u.Nombre,
                UsuarioFoto = u.FotoUrl // 👈 AÑADE ESTO
            })
        .ToListAsync();

    return Ok(lista);
}



        // 🌟 Obtener promedio de calificación
        [HttpGet("calificacion-promedio")]
        public async Task<IActionResult> Promedio(string uuid, [FromQuery] int usuarioId)
        {
            var promedio = await _context.Calificaciones
                .Where(c => c.ProductoUuid == uuid)
                .Select(c => (double?)c.Valor)
                .AverageAsync();

            var valorUsuario = await _context.Calificaciones
                .Where(c => c.ProductoUuid == uuid && c.UsuarioId == usuarioId)
                .Select(c => (int?)c.Valor)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                promedio = promedio ?? 0.0,
                valorUsuario = valorUsuario ?? 0
            });
        }


        // ❤️ Contador de likes y si el usuario ya dio like
        [HttpGet("likes")]
        public async Task<IActionResult> ObtenerLikes(string uuid, [FromQuery] int usuarioId)
        {
            var total = await _context.MeGustas.CountAsync(x => x.ProductoUuid == uuid);
            var yaDioLike = await _context.MeGustas
                .AnyAsync(x => x.ProductoUuid == uuid && x.UsuarioId == usuarioId);

            return Ok(new { likes = total, usuarioDioLike = yaDioLike });
        }
        [HttpPut("comentarios/{id}")]
public async Task<IActionResult> EditarComentario(int id, [FromBody] ComentarioDTO dto)
{
    var comentario = await _context.Comentarios.FindAsync(id);
    if (comentario == null) return NotFound();

    if (comentario.UsuarioId != dto.UsuarioId)
        return Forbid(); // Solo el dueño puede editar

    comentario.Texto = dto.Texto;
    comentario.Fecha = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return Ok(new { mensaje = "Comentario editado" });
}

[HttpDelete("comentarios/{id}")]
public async Task<IActionResult> EliminarComentario(int id, [FromQuery] int usuarioId)
{
    var comentario = await _context.Comentarios.FindAsync(id);
    if (comentario == null) return NotFound();

    if (comentario.UsuarioId != usuarioId)
        return Forbid(); // Solo el dueño puede borrar

    _context.Comentarios.Remove(comentario);
    await _context.SaveChangesAsync();
    return Ok(new { mensaje = "Comentario eliminado" });
}


        
    }
}
