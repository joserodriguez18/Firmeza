using System.Globalization;
using System.Text;
using Firmeza.Core.Entities;
using Firmeza.Core.Interfaces;
using Firmeza.Core.Models;
using Firmeza.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Infrastructure.Services;

public class DocumentoNegocioService : IDocumentoNegocioService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<UsuarioSistema> _userManager;
    private readonly IWebHostEnvironment _environment;

    static DocumentoNegocioService()
    {
        ExcelPackage.License.SetNonCommercialOrganization("Firmeza SAS");
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public DocumentoNegocioService(ApplicationDbContext context, UserManager<UsuarioSistema> userManager, IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
    }

    public async Task<ImportExportResult> ImportarExcelDesnormalizadoAsync(Stream archivoExcel)
    {
        var result = new ImportExportResult();
        using var package = new ExcelPackage(archivoExcel);
        var sheet = package.Workbook.Worksheets.FirstOrDefault() ?? throw new Exception("El archivo no contiene hojas.");
        var rows = sheet.Dimension?.Rows ?? 0;
        var cols = sheet.Dimension?.Columns ?? 0;

        if (rows < 2 || cols < 1)
            throw new Exception("El archivo no contiene datos suficientes.");

        var headers = Enumerable.Range(1, cols)
            .ToDictionary(c => c, c => Normalizar(sheet.Cells[1, c].Text));

        for (var row = 2; row <= rows; row++)
        {
            try
            {
                var values = Enumerable.Range(1, cols)
                    .ToDictionary(c => headers[c], c => sheet.Cells[row, c].Text?.Trim() ?? string.Empty);

                var procesado = false;
                if (TryImportarProducto(values, out var producto))
                {
                    var actualizado = await UpsertProductoAsync(producto!);
                    if (actualizado) result.Actualizados++; else result.Creados++;
                    procesado = true;
                }

                if (TryImportarCliente(values, out var clienteDto))
                {
                    var actualizado = await UpsertClienteAsync(clienteDto!);
                    if (actualizado) result.Actualizados++; else result.Creados++;
                    procesado = true;
                }

                if (procesado)
                    result.Procesados++;
                else
                    result.Errores.Add($"Fila {row}: no se reconocieron columnas de producto o cliente.");
            }
            catch (Exception ex)
            {
                result.Errores.Add($"Fila {row}: {ex.Message}");
            }
        }

        return result;
    }

    public async Task<byte[]> ExportarProductosExcelAsync()
    {
        var productos = await _context.Productos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Productos");
        ws.Cells[1, 1].Value = "Id";
        ws.Cells[1, 2].Value = "Nombre";
        ws.Cells[1, 3].Value = "Codigo";
        ws.Cells[1, 4].Value = "Precio";
        ws.Cells[1, 5].Value = "Stock";

        for (var i = 0; i < productos.Count; i++)
        {
            var p = productos[i];
            var row = i + 2;
            ws.Cells[row, 1].Value = p.Id;
            ws.Cells[row, 2].Value = p.Nombre;
            ws.Cells[row, 3].Value = p.Codigo;
            ws.Cells[row, 4].Value = p.Precio;
            ws.Cells[row, 5].Value = p.Stock;
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportarVentasExcelAsync()
    {
        var ventas = await _context.Ventas.Include(v => v.Cliente).ThenInclude(c => c.Usuario)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .AsNoTracking()
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Ventas");
        ws.Cells[1, 1].Value = "Id";
        ws.Cells[1, 2].Value = "Fecha";
        ws.Cells[1, 3].Value = "Cliente";
        ws.Cells[1, 4].Value = "Total";
        ws.Cells[1, 5].Value = "IVA";

        for (var i = 0; i < ventas.Count; i++)
        {
            var v = ventas[i];
            var row = i + 2;
            ws.Cells[row, 1].Value = v.Id;
            ws.Cells[row, 2].Value = v.Fecha.ToString("yyyy-MM-dd HH:mm");
            ws.Cells[row, 3].Value = v.Cliente?.Usuario?.NombreCompleto ?? string.Empty;
            ws.Cells[row, 4].Value = v.Total;
            ws.Cells[row, 5].Value = v.Iva;
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();
        return await package.GetAsByteArrayAsync();
    }

    public async Task<byte[]> ExportarClientesPdfAsync()
    {
        var clientes = await _context.Clientes.Include(c => c.Usuario).AsNoTracking().OrderBy(c => c.Id).ToListAsync();
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Content().Column(col =>
                {
                    col.Item().Text("Clientes").FontSize(18).SemiBold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Text("Id");
                            h.Cell().Text("Documento");
                            h.Cell().Text("Telefono");
                            h.Cell().Text("Nombre");
                        });
                        foreach (var c in clientes)
                        {
                            table.Cell().Text(c.Id.ToString());
                            table.Cell().Text(c.Documento);
                            table.Cell().Text(c.Telefono);
                            table.Cell().Text(c.Usuario?.NombreCompleto ?? string.Empty);
                        }
                    });
                });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerarReciboVentaPdfAsync(Venta venta)
    {
        var ventaCompleta = await _context.Ventas.Include(v => v.Cliente).ThenInclude(c => c.Usuario)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstAsync(v => v.Id == venta.Id);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(28);
                page.Size(PageSizes.A4);
                page.Header().Text("Recibo de venta").FontSize(18).SemiBold();
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"Venta #{ventaCompleta.Id}");
                    col.Item().Text($"Fecha: {ventaCompleta.Fecha:dd/MM/yyyy HH:mm}");
                    col.Item().Text($"Cliente: {ventaCompleta.Cliente.Usuario.NombreCompleto}");
                    col.Item().Text($"Documento: {ventaCompleta.Cliente.Documento}");
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Text("Producto");
                            h.Cell().Text("Cant.");
                            h.Cell().Text("Valor");
                            h.Cell().Text("Subtotal");
                        });
                        foreach (var d in ventaCompleta.Detalles)
                        {
                            table.Cell().Text(d.Producto.Nombre);
                            table.Cell().Text(d.Cantidad.ToString());
                            table.Cell().Text(d.PrecioUnitario.ToString("C", CultureInfo.GetCultureInfo("es-CO")));
                            table.Cell().Text((d.Cantidad * d.PrecioUnitario).ToString("C", CultureInfo.GetCultureInfo("es-CO")));
                        }
                    });
                    col.Item().AlignRight().Text($"IVA: {ventaCompleta.Iva:C}");
                    col.Item().AlignRight().Text($"Total: {ventaCompleta.Total:C}").Bold();
                });
            });
        }).GeneratePdf();
    }

    private async Task<bool> UpsertProductoAsync(Producto producto)
    {
        var existente = await _context.Productos.FirstOrDefaultAsync(p => p.Codigo == producto.Codigo || p.Nombre == producto.Nombre);
        if (existente == null)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return false;
        }
        existente.Nombre = producto.Nombre;
        existente.Codigo = producto.Codigo;
        existente.Precio = producto.Precio;
        existente.Stock = producto.Stock;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> UpsertClienteAsync(ClienteDto cliente)
    {
        var existente = await _context.Clientes.Include(c => c.Usuario).FirstOrDefaultAsync(c => c.Documento == cliente.Documento || c.Usuario.Email == cliente.Email);
        if (existente == null)
        {
            var usuario = new UsuarioSistema
            {
                UserName = cliente.Email,
                Email = cliente.Email,
                NombreCompleto = cliente.NombreCompleto,
                EmailConfirmed = true
            };
            var password = cliente.Password ?? "Temp123*";
            var created = await _userManager.CreateAsync(usuario, password);
            if (!created.Succeeded)
                throw new Exception(string.Join("; ", created.Errors.Select(e => e.Description)));
            await _userManager.AddToRoleAsync(usuario, "Cliente");
            _context.Clientes.Add(new Cliente { Documento = cliente.Documento, Telefono = cliente.Telefono, Edad = cliente.Edad, UsuarioId = usuario.Id });
            await _context.SaveChangesAsync();
            return false;
        }
        existente.Documento = cliente.Documento;
        existente.Telefono = cliente.Telefono;
        existente.Edad = cliente.Edad;
        if (existente.Usuario != null)
        {
            existente.Usuario.NombreCompleto = cliente.NombreCompleto;
            existente.Usuario.Email = cliente.Email;
            existente.Usuario.UserName = cliente.Email;
            await _userManager.UpdateAsync(existente.Usuario);
        }
        await _context.SaveChangesAsync();
        return true;
    }

    private static bool TryImportarProducto(Dictionary<string, string> values, out Producto? producto)
    {
        producto = null;
        var nombre = Obtener(values, "nombre", "producto", "descripcion");
        var codigo = Obtener(values, "codigo", "sku", "referencia");
        var precioTexto = Obtener(values, "precio", "valor");
        var stockTexto = Obtener(values, "cantidad", "stock", "existencia");

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(codigo))
            return false;

        if (!decimal.TryParse(precioTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) &&
            !decimal.TryParse(precioTexto, NumberStyles.Any, CultureInfo.GetCultureInfo("es-CO"), out precio))
            throw new Exception("El precio es obligatorio y debe ser numérico.");

        if (!int.TryParse(stockTexto, out var stock))
            stock = 0;

        producto = new Producto { Nombre = nombre, Codigo = codigo, Precio = precio, Stock = stock };
        return true;
    }

    private static bool TryImportarCliente(Dictionary<string, string> values, out ClienteDto? cliente)
    {
        cliente = null;
        var documento = Obtener(values, "documento", "cedula", "cc");
        var nombre = Obtener(values, "nombrecliente", "cliente", "nombres");
        var email = Obtener(values, "email", "correo");

        if (string.IsNullOrWhiteSpace(documento) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email))
            return false;

        cliente = new ClienteDto
        {
            Documento = documento,
            NombreCompleto = nombre,
            Email = email,
            Telefono = Obtener(values, "telefono", "celular"),
            Password = Obtener(values, "password", "clave"),
            Edad = int.TryParse(Obtener(values, "edad"), out var edad) ? edad : 0
        };
        return true;
    }

    private static string Obtener(Dictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            var match = values.FirstOrDefault(x => x.Key.Contains(key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Value))
                return match.Value;
        }
        return string.Empty;
    }

    private static string Normalizar(string value)
    {
        var cleaned = new string(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        return cleaned;
    }

    private sealed class ClienteDto
    {
        public string Documento { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Password { get; set; }
        public int Edad { get; set; }
    }
}
