using LecturasJazz.API.Models;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace LecturasJazz.API.Services
{
    public class AuthService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
        {
            _config = config;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public async Task<bool> RegistrarUsuario(Usuario usuario)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("INSERT INTO Usuarios (Nombre, Telefono, PasswordHash) VALUES (@Nombre, @Telefono, @PasswordHash)", connection);

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);

                command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@Telefono", usuario.Telefono);
                command.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                await connection.OpenAsync();
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al registrar usuario: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ActualizarFoto(int userId, string rutaRelativa)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("UPDATE Usuarios SET FotoUrl = @FotoUrl WHERE Id = @Id", connection);

                command.Parameters.AddWithValue("@FotoUrl", rutaRelativa);
                command.Parameters.AddWithValue("@Id", userId);

                await connection.OpenAsync();
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar foto: {ex.Message}");
                return false;
            }
        }

        public async Task<(Usuario? usuario, string? token)> Login(string telefono, string password)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("SELECT * FROM Usuarios WHERE Telefono = @Telefono", connection);
                command.Parameters.AddWithValue("@Telefono", telefono);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var user = new Usuario
                    {
                        Id = (int)reader["Id"],
                        Nombre = (string)reader["Nombre"],
                        Telefono = (string)reader["Telefono"],
                        PasswordHash = (string)reader["PasswordHash"],
                        FotoUrl = reader["FotoUrl"] as string
                    };

                    if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    {
                        var token = GenerarToken(user);
                        return (user, token);
                    }
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en login: {ex.Message}");
                return (null, null);
            }
        }

        private string GenerarToken(Usuario usuario)
        {
            var key = _config["Jwt:Key"] ?? "clave-secreta-super-segura";
            var keyBytes = Encoding.UTF8.GetBytes(key);

            var claims = new[]
            {
                new Claim("id", usuario.Id.ToString()),
                new Claim("nombre", usuario.Nombre),
                new Claim("telefono", usuario.Telefono)
            };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
