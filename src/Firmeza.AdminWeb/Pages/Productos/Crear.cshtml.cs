using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Firmeza.AdminWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.AdminWeb.Pages.Productos;

public class CrearModel : PageModel
{
    private readonly IProductoService _productoService;

    public CrearModel(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [BindProperty]
    public ProductoViewModel Input { get; set; } = new();

    // Variable cruda para capturar el texto del stock y validarlo (Task 7)
    [BindProperty]
    public string StockTexto { get; set; } = string.Empty;

    public string? ErrorExcepcion { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        // 1. Validar primero el formato del Stock usando int.Parse y try-catch (Task 7)
        int stockConvertido;
        try
        {
            if (string.IsNullOrWhiteSpace(StockTexto))
            {
                ErrorExcepcion = "El inventario / stock inicial es obligatorio.";
                return Page();
            }

            stockConvertido = int.Parse(StockTexto);

            if (stockConvertido < 0)
            {
                ErrorExcepcion = "El stock no puede ser un número negativo.";
                return Page();
            }
        }
        catch (FormatException)
        {
            ErrorExcepcion = "Error de Formato: El stock ingresado debe ser un número entero válido (sin letras ni símbolos).";
            return Page();
        }
        catch (OverflowException)
        {
            ErrorExcepcion = "Error de Rango: El número de stock ingresado es demasiado grande para el sistema.";
            return Page();
        }

        // 2. Validar el resto de anotaciones (Nombre, Código, Precio)
        if (!ModelState.IsValid) return Page();

        // 3. Si todo está correcto, guardar en PostgreSQL
        try
        {
            var nuevoProducto = new Producto
            {
                Nombre = Input.Nombre,
                Codigo = Input.Codigo,
                Precio = Input.Precio,
                Stock = stockConvertido
            };

            await _productoService.CrearAsync(nuevoProducto);
            return RedirectToPage("/Productos/Index");
        }
        catch (Exception ex)
        {
            ErrorExcepcion = $"Error de infraestructura: {ex.Message}";
            return Page();
        }
    }
}
