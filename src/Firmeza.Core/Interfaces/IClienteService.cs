using Firmeza.Core.Entities;

namespace Firmeza.Core.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<Cliente>> ObtenerTodosAsync(string? buscar);
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task CrearClienteAsync(Cliente cliente, string email, string password, string nombreCompleto);
    Task ActualizarAsync(Cliente cliente, string nombreCompleto);
    Task EliminarAsync(int id);
    Task<int> ContarTotalAsync();
}