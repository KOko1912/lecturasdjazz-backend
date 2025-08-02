using LecturasJazz.API.Models.ConfigPosicion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LecturasJazz.API.Data; // ✅ Agregado correctamente

namespace LecturasJazz.API.Controllers.ConfigPosicion
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionPosicionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracionPosicionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConfiguracionPosicion>>> GetConfiguracion()
        {
            return await _context.ConfiguracionPosicion.ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> GuardarConfiguracion(List<ConfiguracionPosicion> configuraciones)
        {
            var existentes = await _context.ConfiguracionPosicion.ToListAsync();
            _context.ConfiguracionPosicion.RemoveRange(existentes);
            await _context.ConfiguracionPosicion.AddRangeAsync(configuraciones);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Configuración guardada correctamente." });
        }
    }
}
