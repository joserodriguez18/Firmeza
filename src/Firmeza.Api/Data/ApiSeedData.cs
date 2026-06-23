using Firmeza.Core.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Api.Data;

public static class ApiSeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<UsuarioSistema>>();

        string[] roles = { "Admin", "Cliente" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = "admin@firmeza.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var user = new UsuarioSistema { UserName = adminEmail, Email = adminEmail, NombreCompleto = "Administrador API", EmailConfirmed = true };
            var created = await userManager.CreateAsync(user, "Admin123*");
            if (created.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                context.Administradores.Add(new Administrador { UsuarioId = user.Id, Cargo = "Admin API" });
                await context.SaveChangesAsync();
            }
        }
    }
}
