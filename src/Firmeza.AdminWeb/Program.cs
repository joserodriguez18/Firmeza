using Firmeza.Core.Entities;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la cadena de conexión de PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Configuración de Identity especificando explícitamente Usuario y Rol (Soluciona el error)
builder.Services.AddIdentity<UsuarioSistema, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager<SignInManager<UsuarioSistema>>()
    .AddDefaultTokenProviders();

// 3. Protección de carpetas en Razor Pages y políticas de seguridad
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Dashboard", "RequireAdminRole");
    options.Conventions.AuthorizeFolder("/Productos", "RequireAdminRole");
    options.Conventions.AuthorizeFolder("/Clientes", "RequireAdminRole");
});

// Registrar servicios de negocio
builder.Services.AddScoped<Firmeza.Core.Interfaces.IProductoService, Firmeza.Infrastructure.Services.ProductoService>();
builder.Services.AddScoped<Firmeza.Core.Interfaces.IClienteService, Firmeza.Infrastructure.Services.ClienteService>();
builder.Services.AddScoped<Firmeza.Core.Interfaces.IVentaService, Firmeza.Infrastructure.Services.VentaService>();
builder.Services.AddScoped<Firmeza.Core.Interfaces.IDocumentoNegocioService, Firmeza.Infrastructure.Services.DocumentoNegocioService>();


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

// 4. Configuración del comportamiento de las Cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirección para el rol "Cliente"
});

var app = builder.Build();

// 5. Configuración del Pipeline de Middlewares
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// El orden de estos dos middlewares es obligatorio para la seguridad
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();


// --- AGREGAR ESTO JUSTO ANTES DE app.Run(); ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Ejecuta la inicialización de roles y usuarios
        await Firmeza.AdminWeb.Data.SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar los datos de la BD.");
    }
}

app.Run();
