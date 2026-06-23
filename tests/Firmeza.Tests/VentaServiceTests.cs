using Firmeza.Core.Entities;
using Firmeza.Core.Models;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Firmeza.Tests;

public class VentaServiceTests
{
    // Método auxiliar para crear un DbContext en memoria limpio en cada prueba
    private ApplicationDbContext ObtenerContextoEnMemoria()
    {
        var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            // ESTA ES LA SOLUCIÓN: Le decimos a EF Core que ignore la advertencia de transacciones en la BD en memoria
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(opciones);
    }

    private VentaService ObtenerServicio(ApplicationDbContext contexto, string rootPath)
    {
        var env = new TestWebHostEnvironment { WebRootPath = rootPath };
        return new VentaService(contexto, new FakeDocumentoNegocioService(), env);
    }

    [Fact]
    public async Task RegistrarVentaAsync_DeberiaDescontarStockYCalcularTotalesCorrectamente()
    {
        // 1. ARRANGE (Preparar el escenario)
        var contexto = ObtenerContextoEnMemoria();
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(temp);
        var servicio = ObtenerServicio(contexto, temp);

        // Crear un producto de prueba con 100 unidades en stock
        var producto = new Producto
        {
            Id = 1,
            Nombre = "Saco Cemento Gris",
            Codigo = "CEM-01",
            Precio = 30000m,
            Stock = 100
        };

        // Crear un cliente de prueba
        var cliente = new Cliente
        {
            Id = 1,
            Documento = "123456",
            Telefono = "300123"
        };

        contexto.Productos.Add(producto);
        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        // Estructurar la venta: llevar 5 sacos de cemento
        var venta = new Venta
        {
            Id = 1,
            ClienteId = 1,
            Fecha = DateTime.UtcNow,
            Iva = 28500m, // (30000 * 5) * 0.19
            Total = 178500m, // Subtotal + IVA
            Detalles = new List<DetalleVenta>
            {
                new DetalleVenta
                {
                    ProductoId = 1,
                    Cantidad = 5,
                    PrecioUnitario = 30000m
                }
            }
        };

        // 2. ACT (Ejecutar la acción)
        await servicio.RegistrarVentaAsync(venta);

        // 3. ASSERT (Verificar que los resultados sean los esperados)
        var productoActualizado = await contexto.Productos.FindAsync(1);
        var ventaGuardada = await contexto.Ventas.FindAsync(1);

        // Validar que el stock bajó exactamente de 100 a 95
        Assert.NotNull(productoActualizado);
        Assert.Equal(95, productoActualizado.Stock);

        // Validar que la venta se registró en la base de datos con los montos correctos
        Assert.NotNull(ventaGuardada);
        Assert.Equal(178500m, ventaGuardada.Total);
    }

    [Fact]
    public async Task RegistrarVentaAsync_DeberiaLanzarExcepcion_SiElStockEsInsuficiente()
    {
        // 1. ARRANGE
        var contexto = ObtenerContextoEnMemoria();
        var temp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(temp);
        var servicio = ObtenerServicio(contexto, temp);

        var producto = new Producto
            { Id = 2, Nombre = "Varilla Corrugada", Codigo = "VAR-02", Precio = 50000m, Stock = 3 };
        var cliente = new Cliente { Id = 2, Documento = "78910", Telefono = "311456" };

        contexto.Productos.Add(producto);
        contexto.Clientes.Add(cliente);
        await contexto.SaveChangesAsync();

        // Intentar vender 10 varillas cuando solo hay 3 en stock
        var ventaInvalida = new Venta
        {
            ClienteId = 2,
            Detalles = new List<DetalleVenta>
            {
                new DetalleVenta { ProductoId = 2, Cantidad = 10, PrecioUnitario = 50000m }
            }
        };

        // 2. ACT & ASSERT (Verificar que el sistema tire la excepción controlada y no guarde nada)
        var excepcion = await Assert.ThrowsAsync<Exception>(() => servicio.RegistrarVentaAsync(ventaInvalida));
        Assert.Contains("Stock insuficiente", excepcion.Message);
    }
}

internal sealed class TestWebHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = string.Empty;
    public string ContentRootPath { get; set; } = string.Empty;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string EnvironmentName { get; set; } = Environments.Development;
}

internal sealed class FakeDocumentoNegocioService : Firmeza.Core.Interfaces.IDocumentoNegocioService
{
    public Task<ImportExportResult> ImportarExcelDesnormalizadoAsync(Stream archivoExcel) => Task.FromResult(new ImportExportResult());
    public Task<byte[]> ExportarProductosExcelAsync() => Task.FromResult(Array.Empty<byte>());
    public Task<byte[]> ExportarVentasExcelAsync() => Task.FromResult(Array.Empty<byte>());
    public Task<byte[]> ExportarClientesPdfAsync() => Task.FromResult(Array.Empty<byte>());
    public Task<byte[]> GenerarReciboVentaPdfAsync(Venta venta) => Task.FromResult(Array.Empty<byte>());
}
