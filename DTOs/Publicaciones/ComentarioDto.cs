public class ComentarioDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; } // Asegúrate de agregarlo
    public string UsuarioNombre { get; set; } = "";
    public string Comentario { get; set; } = "";
    public string FotoUsuarioUrl { get; set; } = "";
    public DateTime Fecha { get; set; }
}
