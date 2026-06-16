using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Firmeza.AdminWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.AdminWeb.Pages.Productos;

public class EditarModel : PageModel
{
    private readonly IProductoService _productoService;

    public EditarModel(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [BindProperty]
    public ProductoViewModel Input { get; set; } = new();

    [BindProperty]
    public string StockTexto { get; set; } = string.Empty;

    public string? ErrorExcepcion { get; set; }

    // Carga los datos existentes en el formulario al abrir la página
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var producto = await _productoService.ObtenerPorIdAsync(id);
        if (producto == null)
        {
            return RedirectToPage("/Productos/Index");
        }

        // Mapear los datos de la entidad de la BD al ViewModel de la vista
        Input = new ProductoViewModel
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Codigo = producto.Codigo,
            Precio = producto.Precio
        };

        StockTexto = producto.Stock.ToString();

        return Page();
    }

    // Procesa la actualización en PostgreSQL de forma segura
    public async Task<IActionResult> OnPostAsync()
    {
        // 1. Validar el formato del Stock usando int.Parse (Task 7)
        int stockConvertido;
        try
        {
            if (string.IsNullOrWhiteSpace(StockTexto))
            {
                ErrorExcepcion = "El inventario / stock es obligatorio.";
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
            ErrorExcepcion = "Error de Rango: El número de stock ingresado es demasiado grande.";
            return Page();
        }

        // 2. Validar el resto de anotaciones
        if (!ModelState.IsValid) return Page();

        // 3. Modificar el registro existente
        try
        {
            var productoExistente = await _productoService.ObtenerPorIdAsync(Input.Id);
            if (productoExistente == null)
            {
                ErrorExcepcion = "El producto que intenta editar ya no existe en el sistema.";
                return Page();
            }

            // Actualizar propiedades
            productoExistente.Nombre = Input.Nombre;
            productoExistente.Codigo = Input.Codigo;
            productoExistente.Precio = Input.Precio;
            productoExistente.Stock = stockConvertido;

            await _productoService.ActualizarAsync(productoExistente);
            return RedirectToPage("/Productos/Index");
        }
        catch (Exception ex)
        {
            ErrorExcepcion = $"Error de infraestructura: {ex.Message}";
            return Page();
        }
    }
}
