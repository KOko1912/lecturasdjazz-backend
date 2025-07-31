using LecturasJazz.API.Models;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = new NpgsqlCommand(@"
                    INSERT INTO ""Usuarios"" (""Nombre"", ""Telefono"", ""PasswordHash"") 
                    VALUES (@Nombre, @Telefono, @PasswordHash)", connection);

                command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                command.Parameters.AddWithValue("@Telefono", usuario.Telefono);
                command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash); // ✅ Ya viene hasheado

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al registrar usuario: {ex.Message}");
                return false;
            }
        }

        public async Task<(Usuario? usuario, string? token)> Login(string telefono, string password)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = new NpgsqlCommand(@"
                    SELECT * FROM ""Usuarios"" 
                    WHERE ""Telefono"" = @Telefono", connection);

                command.Parameters.AddWithValue("@Telefono", telefono);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var usuario = new Usuario
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                        Telefono = reader.GetString(reader.GetOrdinal("Telefono")),
                        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                        FotoUrl = reader.IsDBNull(reader.GetOrdinal("FotoUrl")) ? null : reader.GetString(reader.GetOrdinal("FotoUrl"))
                    };

                    if (BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
                    {
                        Console.WriteLine("✅ Contraseña verificada");
                        string token = GenerarToken(usuario);
                        return (usuario, token);
                    }
                    else
                    {
                        Console.WriteLine($"❌ Contraseña incorrecta. Ingresada: {password}, Hash: {usuario.PasswordHash}");
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

        public async Task<bool> ActualizarFoto(int userId, string rutaRelativa)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var command = new NpgsqlCommand(@"
                    UPDATE ""Usuarios"" 
                    SET ""FotoUrl"" = @FotoUrl 
                    WHERE ""Id"" = @Id", connection);

                command.Parameters.AddWithValue("@FotoUrl", rutaRelativa);
                command.Parameters.AddWithValue("@Id", userId);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al actualizar foto: {ex.Message}");
                return false;
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
