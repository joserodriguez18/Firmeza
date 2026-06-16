using Firmeza.Core.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.AdminWeb.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<UsuarioSistema>>();

        // 1. Aplica migraciones pendientes automáticamente al arrancar
        await context.Database.MigrateAsync();

        // 2. Crear los roles obligatorios del sistema
        string[] roles = { "Admin", "Cliente" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 3. Crear el primer Administrador de pruebas si no existe
        var adminEmail = "admin@firmeza.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            var nuevoUsuario = new UsuarioSistema
            {
                UserName = adminEmail,
                Email = adminEmail,
                NombreCompleto = "Administrador Sistema",
                EmailConfirmed = true
            };

            // Contraseña segura temporal para desarrollo
            var resultado = await userManager.CreateAsync(nuevoUsuario, "Admin123*");

            if (resultado.Succeeded)
            {
                // Asignar el rol de Identity
                await userManager.AddToRoleAsync(nuevoUsuario, "Admin");

                // Crear su perfil de negocio en la tabla Administradores
                var perfilAdmin = new Administrador
                {
                    UsuarioId = nuevoUsuario.Id,
                    Cargo = "Super Administrador"
                };

                context.Administradores.Add(perfilAdmin);
                await context.SaveChangesAsync();
            }
        }
    }
}
