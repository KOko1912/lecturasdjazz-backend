using LecturasJazz.API.Data;
using LecturasJazz.API.Models;
using LecturasJazz.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LecturasJazz.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;

    public AuthController(ApplicationDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public class RegisterRequest
    {
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Password { get; set; }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Telefono) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Todos los campos son obligatorios" });
        }

        bool exists = await _context.Usuarios.AnyAsync(u => u.Telefono == request.Telefono);
        if (exists)
            return BadRequest(new { error = "El teléfono ya está registrado" });

        var nuevoUsuario = new Usuario
        {
            Nombre = request.Nombre,
            Telefono = request.Telefono,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password!), // ✅ Hasheo aquí
            FotoUrl = null
        };

        bool creado = await _authService.RegistrarUsuario(nuevoUsuario);
        if (!creado)
            return StatusCode(500, new { message = "No se pudo registrar el usuario" });

        var (usuario, token) = await _authService.Login(request.Telefono!, request.Password!);
        if (usuario == null || token == null)
            return StatusCode(500, new { message = "Usuario creado, pero no se pudo generar token" });

        return Ok(new
        {
            usuario = new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Telefono,
                usuario.FotoUrl
            },
            token
        });
    }

    public class LoginRequest
    {
        public string? Telefono { get; set; }
        public string? Password { get; set; }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Telefono) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Teléfono y contraseña son requeridos" });
        }

        var (usuario, token) = await _authService.Login(request.Telefono, request.Password);
        if (usuario == null || token == null)
            return Unauthorized(new { message = "Teléfono o contraseña incorrectos" });

        return Ok(new
        {
            usuario = new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Telefono,
                usuario.FotoUrl
            },
            token
        });
    }
}
