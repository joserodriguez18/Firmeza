using Firmeza.Core.Entities;

namespace Firmeza.Core.Interfaces;

public interface IProductoService
{
    Task<IEnumerable<Producto>> ObtenerTodosAsync(string? buscar);
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task CrearAsync(Producto producto);
    Task ActualizarAsync(Producto producto);
    Task EliminarAsync(int id);
    Task<int> ContarTotalAsync();
}