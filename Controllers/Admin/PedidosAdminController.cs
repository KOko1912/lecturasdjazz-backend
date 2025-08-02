using LecturasJazz.API.Data;
using LecturasJazz.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LecturasJazz.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
public class PedidosAdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PedidosAdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/admin/pedidosadmin
    [HttpGet]
    public async Task<IActionResult> ObtenerPedidos()
    {
        var pedidos = await _context.Pedidos
            .Include(p => p.Producto)
            .Include(p => p.Usuario)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();

        return Ok(pedidos);
    }

    // PUT: api/admin/pedidosadmin/{id}/completar
    [HttpPut("{id}/completar")]
    public async Task<IActionResult> CompletarPedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
            return NotFound("Pedido no encontrado.");

        pedido.Estado = "Completado";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Pedido actualizado como completado." });
    }

    // PUT: api/admin/pedidosadmin/{id}/rechazar
    [HttpPut("{id}/rechazar")]
    public async Task<IActionResult> RechazarPedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
            return NotFound("Pedido no encontrado.");

        pedido.Estado = "Rechazado";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Pedido rechazado correctamente." });
    }

    // DELETE: api/admin/pedidosadmin/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarPedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
            return NotFound("Pedido no encontrado.");

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Pedido eliminado correctamente." });
    }
}
