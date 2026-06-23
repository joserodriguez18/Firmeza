using AutoMapper;
using Firmeza.Api.Models;
using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly IMapper _mapper;

    public ClientesController(IClienteService clienteService, IMapper mapper)
    {
        _clienteService = clienteService;
        _mapper = mapper;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IEnumerable<ClienteDto>> Get([FromQuery] string? buscar)
        => _mapper.Map<IEnumerable<ClienteDto>>(await _clienteService.ObtenerTodosAsync(buscar));

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> Get(int id)
    {
        var cliente = await _clienteService.ObtenerPorIdAsync(id);
        return cliente == null ? NotFound() : _mapper.Map<ClienteDto>(cliente);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Cliente cliente, [FromQuery] string email, [FromQuery] string password, [FromQuery] string nombreCompleto)
    {
        await _clienteService.CrearClienteAsync(cliente, email, password, nombreCompleto);
        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, ClienteDto dto, [FromQuery] string nombreCompleto)
    {
        if (id != dto.Id) return BadRequest();
        await _clienteService.ActualizarAsync(_mapper.Map<Cliente>(dto), nombreCompleto);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _clienteService.EliminarAsync(id);
        return NoContent();
    }
}
