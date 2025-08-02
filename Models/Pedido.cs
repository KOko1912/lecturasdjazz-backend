using System;
using LecturasJazz.API.Models.Admin;

namespace LecturasJazz.API.Models
{
    public class Pedido
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public int UsuarioId { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? ComprobanteUrl { get; set; }
    public DateTime FechaPedido { get; set; }
    public int? NumeroListaEspera { get; set; }
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public decimal PrecioFinal { get; set; } // 👈 NUEVO CAMPO

    public Producto Producto { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}

}
