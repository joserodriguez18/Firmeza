using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.AdminWeb.Pages.Clientes;

public class IndexModel : PageModel
{
    private readonly IClienteService _clienteService;

    public IndexModel(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    public IEnumerable<Cliente> Clientes { get; set; } = new List<Cliente>();

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    [TempData]
    public string? MensajeExito { get; set; }

    public async Task OnGetAsync()
    {
        Clientes = await _clienteService.ObtenerTodosAsync(Buscar);
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        await _clienteService.EliminarAsync(id);
        MensajeExito = "El cliente y sus credenciales de acceso fueron eliminados del sistema.";
        return RedirectToPage();
    }
}