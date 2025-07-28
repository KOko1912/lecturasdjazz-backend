using System.Text.Json.Serialization;

namespace LecturasJazz.API.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string PasswordHash { get; set; } = string.Empty;

        public string? FotoUrl { get; set; }
    }
}
