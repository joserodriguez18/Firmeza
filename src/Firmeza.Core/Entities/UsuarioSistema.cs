using Microsoft.AspNetCore.Identity;

namespace Firmeza.Core.Entities;

public class UsuarioSistema : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    
    public virtual Cliente? PerfilCliente { get; set; }
    public virtual Administrador? PerfilAdministrador { get; set; }
}