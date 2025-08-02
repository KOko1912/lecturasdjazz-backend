using LecturasJazz.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LecturasJazz.API.Controllers.Usuarios
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _config;

        public UsuarioController(AuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [HttpPost("upload-photo")]
        public async Task<IActionResult> SubirFoto(IFormFile archivo, [FromQuery] int userId)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo no válido");

            var extension = Path.GetExtension(archivo.FileName);
            if (extension == null || (extension != ".jpg" && extension != ".jpeg" && extension != ".png"))
                return BadRequest("Solo se permiten imágenes JPG o PNG");

            // 🛠️ Cambiar ruta a fuera de wwwroot
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var nombreArchivo = $"user_{userId}{extension}";
            var ruta = Path.Combine(uploadsDir, nombreArchivo);

            try
            {
                using var stream = new FileStream(ruta, FileMode.Create);
                await archivo.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al guardar la imagen: {ex.Message}");
            }

            // 👉 La ruta que se guarda en DB
            var rutaRelativa = $"uploads/{nombreArchivo}";
            var ok = await _authService.ActualizarFoto(userId, rutaRelativa);

            if (!ok)
                return StatusCode(500, "Error al guardar ruta en la base de datos");

            return Ok(new { message = "Foto actualizada", ruta = rutaRelativa });
        }

        [HttpGet("detalle/{id}")]
        public async Task<IActionResult> ObtenerUsuario(int id)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var command = new SqlCommand("SELECT Id, Nombre, Telefono, FotoUrl FROM Usuarios WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var usuario = new
                {
                    id = reader.GetInt32(0),
                    nombre = reader.GetString(1),
                    telefono = reader.GetString(2),
                    fotoUrl = reader.IsDBNull(3) ? null : reader.GetString(3)
                };
                return Ok(usuario);
            }

            return NotFound("Usuario no encontrado");
        }
    }
}
