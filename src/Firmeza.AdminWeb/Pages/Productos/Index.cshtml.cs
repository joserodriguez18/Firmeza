using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.AdminWeb.Pages.Productos;

public class IndexModel : PageModel
{
    private readonly IProductoService _productoService;

    public IndexModel(IProductoService productoService)
    {
        _productoService = productoService;
    }

    // Almacena la lista de productos filtrados para pintarla en la tabla
    public IEnumerable<Producto> Productos { get; set; } = new List<Producto>();

    // Enlaza el input de búsqueda automáticamente de forma bidireccional
    [BindProperty(SupportsGet = true)] public string? Buscar { get; set; }

    [TempData] public string? MensajeExito { get; set; }

    // Se ejecuta al cargar la página (Soporta filtrado - Task 6)
    public async Task OnGetAsync()
    {
        Productos = await _productoService.ObtenerTodosAsync(Buscar);
    }

    // Procesa la eliminación segura desde la misma tabla
    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsync(id);
        if (producto == null)
        {
            return RedirectToPage();
        }

        // Invoca al servicio que se comunica con PostgreSQL
        await _productoService.EliminarAsync(id);

        // Mensaje temporal que se mostrará en la alerta superior
        MensajeExito = $"El material '{producto.Nombre}' fue eliminado exitosamente del catálogo.";

        return RedirectToPage();
    }
}