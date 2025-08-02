using LecturasJazz.API.Data;
using LecturasJazz.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LecturasJazz.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public PedidosController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost]
    public async Task<IActionResult> CrearPedido([FromForm] PedidoDto dto)
    {
        if (dto.Comprobante == null || dto.Comprobante.Length == 0)
            return BadRequest("Archivo inválido.");

        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.Uuid.ToString().ToLower() == dto.UuidProducto.ToLower());

        if (producto == null)
            return NotFound("Producto no encontrado.");

        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int usuarioId))
            return Unauthorized("Usuario no autenticado o ID inválido.");

        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        var pedidoExistente = await _context.Pedidos
            .Where(p => p.ProductoId == producto.Id && p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.FechaPedido)
            .FirstOrDefaultAsync();

        if (pedidoExistente != null)
        {
            if (pedidoExistente.Estado == "Pendiente" && (DateTime.Now - pedidoExistente.FechaPedido).TotalMinutes < 30)
            {
                return Conflict("Ya tienes un pedido pendiente para este producto.");
            }
        }

        var comprobantesPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "comprobantes");
        Directory.CreateDirectory(comprobantesPath);

        var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(dto.Comprobante.FileName);
        var rutaRelativa = Path.Combine("uploads", "comprobantes", nombreArchivo);
        var rutaAbsoluta = Path.Combine(Directory.GetCurrentDirectory(), rutaRelativa);

        using (var stream = new FileStream(rutaAbsoluta, FileMode.Create))
        {
            await dto.Comprobante.CopyToAsync(stream);
        }

        var cantidadPendientes = await _context.Pedidos.CountAsync(p => p.Estado == "Pendiente");

        // Cálculo del precio final con descuento
        var precioBase = producto.Precio;
        var descuento = producto.EnOferta ? producto.PorcentajeOferta : 0;
        var precioFinal = Math.Round(precioBase * (1 - (descuento / 100m)), 2);

        var nuevoPedido = new Pedido
        {
            ProductoId = producto.Id,
            UsuarioId = usuarioId,
            Estado = "Pendiente",
            FechaPedido = DateTime.Now,
            NumeroListaEspera = cantidadPendientes + 1,
            ComprobanteUrl = "/" + rutaRelativa.Replace("\\", "/"),
            Uuid = Guid.NewGuid(),
            PrecioFinal = precioFinal
        };

        _context.Pedidos.Add(nuevoPedido);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            uuid = nuevoPedido.Uuid,
            cliente = new { nombre = usuario.Nombre, telefono = usuario.Telefono },
            producto = new
            {
                titulo = producto.Titulo,
                descripcion = producto.Descripcion,
                precio = producto.Precio,
                enOferta = producto.EnOferta,
                porcentajeOferta = producto.PorcentajeOferta
            }
        });
    }

    [HttpGet("{uuid}")]
    public async Task<IActionResult> ObtenerPedidoPorUuid(Guid uuid)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Producto)
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Uuid == uuid);

        if (pedido == null)
            return NotFound("Pedido no encontrado.");

        return Ok(new
        {
            producto = new
            {
                titulo = pedido.Producto.Titulo,
                descripcion = pedido.Producto.Descripcion,
                precio = pedido.Producto.Precio,
                enOferta = pedido.Producto.EnOferta,
                porcentajeOferta = pedido.Producto.PorcentajeOferta,
                imagenUrl = pedido.Producto.ImagenUrl,
                uuid = pedido.Producto.Uuid
            },
            usuario = new
            {
                nombre = pedido.Usuario.Nombre,
                telefono = pedido.Usuario.Telefono
            },
            pedido.Id,
            pedido.Estado,
            pedido.ComprobanteUrl,
            pedido.FechaPedido,
            pedido.NumeroListaEspera,
            pedido.Uuid,
            pedido.PrecioFinal
        });
    }

    [HttpGet("mis-pedidos")]
    public async Task<IActionResult> ObtenerMisPedidos()
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int usuarioId))
            return Unauthorized("Usuario no autenticado o ID inválido.");

        var pedidos = await _context.Pedidos
            .Include(p => p.Producto)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.FechaPedido)
            .Select(p => new
            {
                p.Uuid,
                p.Estado,
                p.FechaPedido,
                p.ComprobanteUrl,
                p.PrecioFinal,
                producto = new
                {
                    p.Producto.Titulo,
                    p.Producto.Uuid
                }
            })
            .ToListAsync();

        return Ok(pedidos);
    }

    [HttpPut("subir-comprobante")]
    public async Task<IActionResult> SubirComprobante([FromForm] IFormFile comprobante, [FromForm] string uuidPedido)
    {
        if (string.IsNullOrWhiteSpace(uuidPedido) || comprobante == null || comprobante.Length == 0)
            return BadRequest("Faltan datos o archivo inválido.");

        if (!Guid.TryParse(uuidPedido, out Guid pedidoUuid))
            return BadRequest("UUID de pedido inválido.");

        var userIdClaim = User.FindFirst("id")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out int usuarioId))
            return Unauthorized("Usuario no autenticado.");

        var pedido = await _context.Pedidos
            .Include(p => p.Producto)
            .FirstOrDefaultAsync(p => p.Uuid == pedidoUuid && p.UsuarioId == usuarioId);

        if (pedido == null)
            return NotFound("Pedido no encontrado.");

        if (pedido.Estado != "Pendiente")
            return BadRequest("Solo puedes actualizar el comprobante si el pedido está pendiente.");

        var comprobantesPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "comprobantes");
        Directory.CreateDirectory(comprobantesPath);

        var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(comprobante.FileName);
        var rutaRelativa = Path.Combine("uploads", "comprobantes", nombreArchivo);
        var rutaAbsoluta = Path.Combine(Directory.GetCurrentDirectory(), rutaRelativa);

        using (var stream = new FileStream(rutaAbsoluta, FileMode.Create))
        {
            await comprobante.CopyToAsync(stream);
        }

        pedido.ComprobanteUrl = "/" + rutaRelativa.Replace("\\", "/");
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Comprobante actualizado correctamente.",
            ruta = pedido.ComprobanteUrl
        });
    }

    public class PedidoDto
    {
        public string UuidProducto { get; set; } = string.Empty;
        public IFormFile Comprobante { get; set; } = null!;
    }
}
