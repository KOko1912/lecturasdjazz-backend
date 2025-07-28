using LecturasJazz.API.Data;
using LecturasJazz.API.Models;

using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace LecturasJazz.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AuthAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AdminRegisterRequest request)
        {
            if (_context.AdminUsers.Any(a => a.Telefono == request.Telefono))
            {
                return BadRequest(new { message = "El teléfono ya está registrado" });
            }

            var admin = new AdminUser
            {
                Nombre = request.Nombre,
                Telefono = request.Telefono,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FechaCreacion = DateTime.UtcNow
            };

            _context.AdminUsers.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Administrador creado" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AdminLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Telefono) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Debe ingresar teléfono y contraseña." });
            }

            var admin = _context.AdminUsers.FirstOrDefault(a => a.Telefono == request.Telefono);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            return Ok(new
            {
                admin = new
                {
                    admin.Id,
                    admin.Nombre
                }
            });
        }
    }

    public class AdminRegisterRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AdminLoginRequest
    {
        public string Telefono { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
