namespace Firmeza.Core.Entities;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public int ClienteId { get; set; }
    public virtual Cliente Cliente { get; set; } = null!;
    public decimal Total { get; set; }
    public decimal Iva { get; set; }
    public virtual ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}