using AutoMapper;
using Firmeza.Api.Models;
using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly IMapper _mapper;

    public ProductosController(IProductoService productoService, IMapper mapper)
    {
        _productoService = productoService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IEnumerable<ProductoDto>> Get([FromQuery] string? buscar)
        => _mapper.Map<IEnumerable<ProductoDto>>(await _productoService.ObtenerTodosAsync(buscar));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> Get(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsync(id);
        return producto == null ? NotFound() : _mapper.Map<ProductoDto>(producto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> Post(ProductoDto dto)
    {
        var producto = _mapper.Map<Producto>(dto);
        await _productoService.CrearAsync(producto);
        return CreatedAtAction(nameof(Get), new { id = producto.Id }, _mapper.Map<ProductoDto>(producto));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, ProductoDto dto)
    {
        if (id != dto.Id) return BadRequest();
        await _productoService.ActualizarAsync(_mapper.Map<Producto>(dto));
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productoService.EliminarAsync(id);
        return NoContent();
    }
}
