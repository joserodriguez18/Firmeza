using Firmeza.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Firmeza.AdminWeb.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<UsuarioSistema> _signInManager;
    private readonly UserManager<UsuarioSistema> _userManager;

    public LoginModel(SignInManager<UsuarioSistema> signInManager, UserManager<UsuarioSistema> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Buscar el usuario por correo
        var usuario = await _userManager.FindByEmailAsync(Input.Email);
        if (usuario == null)
        {
            ErrorMessage = "Credenciales incorrectas.";
            return Page();
        }

        // Validar el Rol Obligatorio (Task 4: Bloquear clientes en Razor)
        var esAdmin = await _userManager.IsInRoleAsync(usuario, "Admin");
        if (!esAdmin)
        {
            ErrorMessage = "Acceso denegado. Este panel es exclusivo para administradores.";
            return Page();
        }

        // Intentar iniciar sesión por cookies tradicionales
        var resultado = await _signInManager.PasswordSignInAsync(usuario.UserName!, Input.Password, isPersistent: false, lockoutOnFailure: false);

        if (resultado.Succeeded)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        ErrorMessage = "Usuario o contraseña inválidos.";
        return Page();
    }
}
