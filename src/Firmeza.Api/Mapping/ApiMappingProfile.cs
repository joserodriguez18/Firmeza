using AutoMapper;
using Firmeza.Api.Models;
using Firmeza.Core.Entities;

namespace Firmeza.Api.Mapping;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<Producto, ProductoDto>().ReverseMap();
        CreateMap<Cliente, ClienteDto>().ReverseMap();
        CreateMap<DetalleVenta, DetalleVentaDto>().ReverseMap();
        CreateMap<Venta, VentaDto>().ReverseMap();
    }
}
