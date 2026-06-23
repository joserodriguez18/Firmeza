using AutoMapper;
using Firmeza.Api.Models;
using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventaService;
    private readonly IMapper _mapper;

    public VentasController(IVentaService ventaService, IMapper mapper)
    {
        _ventaService = ventaService;
        _mapper = mapper;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IEnumerable<VentaDto>> Get()
        => _mapper.Map<IEnumerable<VentaDto>>(await _ventaService.ObtenerTodasAsync());

    [Authorize(Roles = "Admin,Cliente")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> Get(int id)
    {
        var venta = await _ventaService.ObtenerPorIdAsync(id);
        return venta == null ? NotFound() : _mapper.Map<VentaDto>(venta);
    }

    [Authorize(Roles = "Cliente")]
    [HttpPost]
    public async Task<IActionResult> Post(VentaDto dto)
    {
        var venta = _mapper.Map<Venta>(dto);
        await _ventaService.RegistrarVentaAsync(venta);
        return Ok(_mapper.Map<VentaDto>(venta));
    }
}
