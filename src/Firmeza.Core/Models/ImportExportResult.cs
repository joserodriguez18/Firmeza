namespace Firmeza.Core.Models;

public class ImportExportResult
{
    public int Procesados { get; set; }
    public int Actualizados { get; set; }
    public int Creados { get; set; }
    public List<string> Errores { get; set; } = new();
}
