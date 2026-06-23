using Firmeza.Core.Entities;
using Firmeza.Core.Models;

namespace Firmeza.Core.Interfaces;

public interface IDocumentoNegocioService
{
    Task<ImportExportResult> ImportarExcelDesnormalizadoAsync(Stream archivoExcel);
    Task<byte[]> ExportarProductosExcelAsync();
    Task<byte[]> ExportarVentasExcelAsync();
    Task<byte[]> ExportarClientesPdfAsync();
    Task<byte[]> GenerarReciboVentaPdfAsync(Venta venta);
}
