using Firmeza.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Firmeza.AdminWeb.Pages.Documentos;

public class IndexModel : PageModel
{
    private readonly IDocumentoNegocioService _documentoService;
    private readonly IWebHostEnvironment _environment;

    public IndexModel(IDocumentoNegocioService documentoService, IWebHostEnvironment environment)
    {
        _documentoService = documentoService;
        _environment = environment;
    }

    [BindProperty]
    public IFormFile? ArchivoExcel { get; set; }

    public string? Mensaje { get; set; }
    public string? Error { get; set; }
    public List<string> Recibos { get; set; } = new();

    public void OnGet()
    {
        var folder = Path.Combine(_environment.WebRootPath, "recibos");
        if (Directory.Exists(folder))
            Recibos = Directory.GetFiles(folder, "*.pdf").Select(Path.GetFileName).Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToList();
    }

    public async Task<IActionResult> OnPostImportarAsync()
    {
        if (ArchivoExcel == null || ArchivoExcel.Length == 0)
        {
            Error = "Debes seleccionar un archivo .xlsx.";
            return Page();
        }

        await using var stream = ArchivoExcel.OpenReadStream();
        var resultado = await _documentoService.ImportarExcelDesnormalizadoAsync(stream);
        Mensaje = $"Procesados: {resultado.Procesados}. Creados: {resultado.Creados}. Actualizados: {resultado.Actualizados}. Errores: {resultado.Errores.Count}.";
        ViewData["Errores"] = resultado.Errores;
        OnGet();
        return Page();
    }

    public async Task<IActionResult> OnGetExportarProductosExcelAsync()
    {
        var bytes = await _documentoService.ExportarProductosExcelAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "productos.xlsx");
    }

    public async Task<IActionResult> OnGetExportarClientesPdfAsync()
    {
        var bytes = await _documentoService.ExportarClientesPdfAsync();
        return File(bytes, "application/pdf", "clientes.pdf");
    }

    public async Task<IActionResult> OnGetExportarVentasExcelAsync()
    {
        var bytes = await _documentoService.ExportarVentasExcelAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ventas.xlsx");
    }

    public IActionResult OnGetDescargarRecibo(string nombre)
    {
        var path = Path.Combine(_environment.WebRootPath, "recibos", nombre);
        if (!System.IO.File.Exists(path))
            return NotFound();
        return PhysicalFile(path, "application/pdf", nombre);
    }
}
