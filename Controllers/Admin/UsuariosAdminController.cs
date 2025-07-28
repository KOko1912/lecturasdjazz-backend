using LecturasJazz.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LecturasJazz.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    public class UsuariosAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("con-metricas")]
        public async Task<IActionResult> ObtenerUsuariosConMetricas()
        {
            var usuariosConDatos = await _context.Usuarios
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Telefono,
                    u.FotoUrl,
                    PedidosTotales = _context.Pedidos
                        .Count(p => p.UsuarioId == u.Id && p.Estado == "Completado"),
                    TotalGastado = _context.Pedidos
                        .Where(p => p.UsuarioId == u.Id && p.Estado == "Completado")
                        .Sum(p => (decimal?)p.PrecioFinal) ?? 0
                })
                .OrderByDescending(u => u.TotalGastado)
                .ToListAsync();

            return Ok(usuariosConDatos);
        }
    }
}
