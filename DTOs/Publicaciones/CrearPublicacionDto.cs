namespace LecturasJazz.API.DTOs.Publicaciones
{
    public class CrearPublicacionDto
    {
        public required string Descripcion { get; set; }
        public required string MediaUrl { get; set; }
        public bool EsVideo { get; set; }
        public int? ProductoId { get; set; }
        public int? DescuentoRelacionado { get; set; }
    }
}
