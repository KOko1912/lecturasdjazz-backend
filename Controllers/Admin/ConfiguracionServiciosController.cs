using LecturasJazz.API.Data;
using LecturasJazz.API.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LecturasJazz.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/configuracion-servicios")]
    public class ConfiguracionServiciosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConfiguracionServiciosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConfiguracionServicios>>> GetAll()
        {
            return await _context.ConfiguracionServicios
                .OrderBy(c => c.Orden)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> Crear(ConfiguracionServicios config)
        {
            _context.ConfiguracionServicios.Add(config);
            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Editar(int id, ConfiguracionServicios config)
        {
            var existente = await _context.ConfiguracionServicios.FindAsync(id);
            if (existente == null) return NotFound();

            existente.Titulo = config.Titulo;
            existente.Subtitulo = config.Subtitulo;
            existente.ProductosUuids = config.ProductosUuids;
            existente.Orden = config.Orden;

            await _context.SaveChangesAsync();
            return Ok(existente);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Eliminar(int id)
        {
            var config = await _context.ConfiguracionServicios.FindAsync(id);
            if (config == null) return NotFound();

            _context.ConfiguracionServicios.Remove(config);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
