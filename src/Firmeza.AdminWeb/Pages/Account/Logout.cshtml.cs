using Firmeza.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.AdminWeb.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<UsuarioSistema> _signInManager;

    public LogoutModel(SignInManager<UsuarioSistema> signInManager)
    {
        _signInManager = signInManager;
    }

    // Bloqueamos accesos directos por GET para evitar cierres de sesión accidentales
    public void OnGet() { }

    // El cierre de sesión seguro en Identity siempre debe procesarse por POST
    public async Task<IActionResult> OnPostAsync()
    {
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Account/Login");
    }
}