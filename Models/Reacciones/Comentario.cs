namespace LecturasJazz.API.Models.Reacciones;

public class Comentario
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string ProductoUuid { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
}
