using Firmeza.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.AdminWeb.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly IProductoService _productoService;
    private readonly IClienteService _clienteService;
    private readonly IVentaService _ventaService; // <-- Inyectar

    public int TotalProductos { get; set; }
    public int TotalClientes { get; set; }
    public int TotalVentas { get; set; } // <-- Métrica viva

    public IndexModel(IProductoService productoService, IClienteService clienteService, IVentaService ventaService)
    {
        _productoService = productoService;
        _clienteService = clienteService;
        _ventaService = ventaService; // <-- Enlazar
    }

    public async Task OnGetAsync()
    {
        TotalProductos = await _productoService.ContarTotalAsync();
        TotalClientes = await _clienteService.ContarTotalAsync();
        TotalVentas = await _ventaService.ContarTotalAsync(); // <-- Consulta PostgreSQL
    }
}