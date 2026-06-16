using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Firmeza.AdminWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.AdminWeb.Pages.Clientes;

public class CrearModel : PageModel
{
    private readonly IClienteService _clienteService;

    public CrearModel(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [BindProperty]
    public ClienteViewModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            var nuevoCliente = new Cliente
            {
                Documento = Input.Documento,
                Telefono = Input.Telefono,
                Edad = Input.Edad
            };

            // Ejecuta la transacción dual (Identity User + Cliente Profile)
            await _clienteService.CrearClienteAsync(nuevoCliente, Input.Email, Input.Password, Input.NombreCompleto);
            
            return RedirectToPage("/Clientes/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}