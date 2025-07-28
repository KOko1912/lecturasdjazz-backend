public class MeGusta
{
    public int Id { get; set; }
    public string ProductoUuid { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } // 👈 No debe ser null
    public int UsuarioId { get; set; }
}
