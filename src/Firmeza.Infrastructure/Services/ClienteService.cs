using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<UsuarioSistema> _userManager;

    public ClienteService(ApplicationDbContext context, UserManager<UsuarioSistema> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IEnumerable<Cliente>> ObtenerTodosAsync(string? buscar)
    {
        var query = _context.Clientes.Include(c => c.Usuario).AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.ToLower();
            query = query.Where(c => c.Documento.Contains(buscar) || 
                                     c.Usuario.NombreCompleto.ToLower().Contains(buscar) || 
                                     c.Usuario.Email!.ToLower().Contains(buscar));
        }
        
        return await query.ToListAsync();
    }

    public async Task<Cliente?> ObtenerPorIdAsync(int id) => 
        await _context.Clientes.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.Id == id);

    public async Task CrearClienteAsync(Cliente cliente, string email, string password, string nombreCompleto)
    {
        var nuevoUsuario = new UsuarioSistema
        {
            UserName = email,
            Email = email,
            NombreCompleto = nombreCompleto,
            EmailConfirmed = true
        };

        var resultadoUser = await _userManager.CreateAsync(nuevoUsuario, password);
        if (resultadoUser.Succeeded)
        {
            await _userManager.AddToRoleAsync(nuevoUsuario, "Cliente");
            
            cliente.UsuarioId = nuevoUsuario.Id;
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }
        else
        {
            var errores = string.Join(", ", resultadoUser.Errors.Select(e => e.Description));
            throw new Exception($"No se pudo crear el usuario de Identity: {errores}");
        }
    }

    public async Task ActualizarAsync(Cliente cliente, string nombreCompleto)
    {
        _context.Entry(cliente).State = EntityState.Modified;
        
        var usuario = await _userManager.FindByIdAsync(cliente.UsuarioId);
        if (usuario != null)
        {
            usuario.NombreCompleto = nombreCompleto;
            await _userManager.UpdateAsync(usuario);
        }

        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            var usuario = await _userManager.FindByIdAsync(cliente.UsuarioId);
            
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            if (usuario != null)
            {
                await _userManager.DeleteAsync(usuario);
            }
        }
    }

    public async Task<int> ContarTotalAsync() => await _context.Clientes.CountAsync();
}
