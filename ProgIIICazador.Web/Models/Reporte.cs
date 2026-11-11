namespace ProgIIICazador.Web.Models;

public class Reporte
{
    public int Id { get; set; }
    public int OfertaId { get; set; }
    public Oferta? Oferta { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public EstadoReporte Estado { get; set; } = EstadoReporte.Pendiente;
}

public enum EstadoReporte
{
    Pendiente = 0,
    EnRevision = 1,
    Resuelto = 2,
    Descartado = 3
}