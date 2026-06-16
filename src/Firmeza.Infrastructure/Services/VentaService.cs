using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Firmeza.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class VentaService : IVentaService
{
    private readonly ApplicationDbContext _context;

    public VentaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Venta>> ObtenerTodasAsync() =>
        await _context.Ventas.Include(v => v.Cliente).ThenInclude(c => c.Usuario).OrderByDescending(v => v.Fecha).ToListAsync();

    public async Task<Venta?> ObtenerPorIdAsync(int id) =>
        await _context.Ventas.Include(v => v.Cliente).ThenInclude(c => c.Usuario)
                             .Include(v => v.Detalles).ThenInclude(d => d.Producto)
                             .FirstOrDefaultAsync(v => v.Id == id);

    public async Task RegistrarVentaAsync(Venta venta)
    {
        using var transaccion = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Procesar cada detalle y descontar inventario
            foreach (var detalle in venta.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId) 
                    ?? throw new Exception($"El producto con ID {detalle.ProductoId} no existe.");

                if (producto.Stock < detalle.Cantidad)
                    throw new Exception($"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}");

                // Descontar inventario corporativo
                producto.Stock -= detalle.Cantidad;
            }

            // 2. Guardar la venta en PostgreSQL
            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    public async Task<int> ContarTotalAsync() => await _context.Ventas.CountAsync();
}
