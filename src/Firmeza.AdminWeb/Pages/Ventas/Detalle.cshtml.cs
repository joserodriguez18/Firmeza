using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.AdminWeb.Pages.Ventas;

public class DetalleModel : PageModel
{
    private readonly IVentaService _ventaService;

    public DetalleModel(IVentaService ventaService)
    {
        _ventaService = ventaService;
    }

    // Almacena la entidad de la venta con todos sus amarres
    public Venta? Venta { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Venta = await _ventaService.ObtenerPorIdAsync(id);

        if (Venta == null)
        {
            return RedirectToPage("/Ventas/Index");
        }

        return Page();
    }
}