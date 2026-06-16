using Firmeza.Core.Entities;

namespace Firmeza.Core.Interfaces;

public interface IVentaService
{
    Task<IEnumerable<Venta>> ObtenerTodasAsync();
    Task<Venta?> ObtenerPorIdAsync(int id);
    Task RegistrarVentaAsync(Venta venta);
    Task<int> ContarTotalAsync();
}