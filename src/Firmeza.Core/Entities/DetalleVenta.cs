namespace Firmeza.Core.Entities;

public class DetalleVenta
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public virtual Venta Venta { get; set; } = null!;
    public int ProductoId { get; set; }
    public virtual Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}