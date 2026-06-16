using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.AdminWeb.Pages.Ventas;

public class IndexModel : PageModel
{
    private readonly IVentaService _ventaService;

    public IndexModel(IVentaService ventaService)
    {
        _ventaService = ventaService;
    }

    public IEnumerable<Venta> Ventas { get; set; } = new List<Venta>();

    public async Task OnGetAsync()
    {
        Ventas = await _ventaService.ObtenerTodasAsync();
    }
}