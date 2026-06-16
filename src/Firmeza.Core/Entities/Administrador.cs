namespace Firmeza.Core.Entities;

public class Administrador
{
    public int Id { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public int Edad { get; set; }
    public string Cargo { get; set; }

    public string UsuarioId { get; set; } = string.Empty;
    public virtual UsuarioSistema Usuario { get; set; } = null!;
}