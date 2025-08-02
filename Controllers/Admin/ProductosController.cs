using LecturasJazz.API.Data;
using LecturasJazz.API.Models.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Lecturasjazz.API.Controllers.Admin
{   [Authorize] 
    [ApiController]
    [Route("api/admin/productos")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductosController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var productos = await _context.Productos.ToListAsync();
            return Ok(productos);
        }

        [HttpGet("{uuid}")]
        public async Task<IActionResult> GetProducto(Guid uuid)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Uuid == uuid);
            if (producto == null)
                return NotFound();

            producto.ConteoVisitas++;
            await _context.SaveChangesAsync();

            return Ok(producto);
        }

        [HttpPost("upload-imagen")]
        public async Task<IActionResult> UploadImagen([FromForm] IFormFile imagen, [FromForm] string nombreUsuario)
        {
            if (imagen == null || imagen.Length == 0)
                return BadRequest("No se subió ninguna imagen.");

            var carpetaDestino = Path.Combine(_env.WebRootPath ?? "wwwroot", "imagenes", "productos", nombreUsuario);
            Directory.CreateDirectory(carpetaDestino);

            var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
            var rutaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            var url = $"/imagenes/productos/{nombreUsuario}/{nombreArchivo}";
            return Ok(new { imageUrl = url });
        }

        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] Producto producto)
        {
            if (producto == null || string.IsNullOrWhiteSpace(producto.Titulo))
                return BadRequest("Datos de producto inválidos.");

            var nuevoProducto = new Producto
            {
                Titulo = producto.Titulo,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                EnOferta = producto.EnOferta,
                PorcentajeOferta = producto.PorcentajeOferta,
                ImagenUrl = producto.ImagenUrl,
                Stock = producto.Stock,
                Activo = true,
                Categoria = producto.Categoria,
                Destacado = producto.Destacado,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = null,
                Uuid = Guid.NewGuid(),
                ConteoVisitas = 0
            };

            _context.Productos.Add(nuevoProducto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducto), new { uuid = nuevoProducto.Uuid }, nuevoProducto);
        }

        [HttpPut("{uuid}")]
        public async Task<IActionResult> ActualizarProducto(Guid uuid, [FromBody] Producto producto)
        {
            var existente = await _context.Productos.FirstOrDefaultAsync(p => p.Uuid == uuid);
            if (existente == null)
                return NotFound();

            existente.Titulo = producto.Titulo;
            existente.Descripcion = producto.Descripcion;
            existente.Precio = producto.Precio;
            existente.EnOferta = producto.EnOferta;
            existente.PorcentajeOferta = producto.PorcentajeOferta;
            existente.ImagenUrl = producto.ImagenUrl;
            existente.Stock = producto.Stock;
            existente.Activo = producto.Activo;
            existente.Categoria = producto.Categoria;
            existente.Destacado = producto.Destacado;
            existente.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{uuid}")]
        public async Task<IActionResult> EliminarProducto(Guid uuid)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Uuid == uuid);
            if (producto == null)
                return NotFound();

            producto.Activo = false;
            producto.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{uuid}/permanente")]
        public async Task<IActionResult> EliminarProductoPermanentemente(Guid uuid)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Uuid == uuid);
            if (producto == null)
                return NotFound();

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            // Intenta borrar la imagen asociada
            if (!string.IsNullOrEmpty(producto.ImagenUrl))
            {
                var rutaImagen = Path.Combine(_env.WebRootPath ?? "wwwroot", producto.ImagenUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(rutaImagen))
                    System.IO.File.Delete(rutaImagen);
            }

            return NoContent();
        }

        [HttpPatch("{uuid}/toggle-activo")]
        public async Task<IActionResult> ToggleActivo(Guid uuid)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Uuid == uuid);
            if (producto == null)
                return NotFound();

            producto.Activo = !producto.Activo;
            producto.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
