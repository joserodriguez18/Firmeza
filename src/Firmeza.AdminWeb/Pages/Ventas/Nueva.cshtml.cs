using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Firmeza.AdminWeb.Pages.Ventas;

public class NuevaModel : PageModel
{
    private readonly IVentaService _ventaService;
    private readonly IClienteService _clienteService;
    private readonly IProductoService _productoService;

    public NuevaModel(IVentaService ventaService, IClienteService clienteService, IProductoService productoService)
    {
        _ventaService = ventaService;
        _clienteService = clienteService;
        _productoService = productoService;
    }

    public IEnumerable<Cliente> Clientes { get; set; } = new List<Cliente>();
    public IEnumerable<Producto> Productos { get; set; } = new List<Producto>();

    [BindProperty]
    public int ClienteIdSeleccionado { get; set; }

    // Aquí recibiremos la lista de productos serializada desde JavaScript
    [BindProperty]
    public string ProductosJson { get; set; } = string.Empty;

    public string? ErrorVenta { get; set; }

    public async Task OnGetAsync()
    {
        Clientes = await _clienteService.ObtenerTodosAsync(null);
        Productos = await _productoService.ObtenerTodosAsync(null);
    }

    // Estructura temporal para mapear el JSON enviado por el cliente
    public class ItemCarrito
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(ProductosJson) || ProductosJson == "[]")
        {
            ErrorVenta = "Debes agregar al menos un producto al carrito antes de facturar.";
            await OnGetAsync();
            return Page();
        }

        try
        {
            // Deserializar los productos elegidos por el usuario
            var items = JsonSerializer.Deserialize<List<ItemCarrito>>(ProductosJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (items == null || !items.Any())
                throw new Exception("El formato del carrito de compras no es válido.");

            var nuevaVenta = new Venta
            {
                ClienteId = ClienteIdSeleccionado,
                Fecha = DateTime.UtcNow,
                Detalles = new List<DetalleVenta>()
            };

            decimal subtotalVenta = 0;

            // Procesar y validar cada producto del listado
            foreach (var item in items)
            {
                var prod = await _productoService.ObtenerPorIdAsync(item.ProductoId) 
                    ?? throw new Exception($"Producto con ID {item.ProductoId} no encontrado.");

                if (item.Cantidad <= 0)
                    throw new Exception($"La cantidad para '{prod.Nombre}' debe ser mayor a cero.");

                var detalle = new DetalleVenta
                {
                    ProductoId = prod.Id,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = prod.Precio
                };

                subtotalVenta += (prod.Precio * item.Cantidad);
                nuevaVenta.Detalles.Add(detalle);
            }

            // Cálculos finales basados en el subtotal de todos los productos
            nuevaVenta.Iva = subtotalVenta * 0.19m; // 19% IVA
            nuevaVenta.Total = subtotalVenta + nuevaVenta.Iva;

            // El servicio guarda la orden y descuenta el stock de todos los materiales en bloque
            await _ventaService.RegistrarVentaAsync(nuevaVenta);
            return RedirectToPage("/Ventas/Index");
        }
        catch (Exception ex)
        {
            ErrorVenta = ex.Message;
            await OnGetAsync();
            return Page();
        }
    }
}
