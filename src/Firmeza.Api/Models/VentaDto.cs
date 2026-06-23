namespace Firmeza.Api.Models;

public record VentaDto(int Id, DateTime Fecha, int ClienteId, decimal Total, decimal Iva, List<DetalleVentaDto> Detalles);
