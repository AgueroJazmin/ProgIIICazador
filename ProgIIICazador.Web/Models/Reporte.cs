using System.ComponentModel.DataAnnotations;

namespace ProgIIICazador.Web.Models;

public class Reporte
{
    public int Id { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una oferta válida.")] public int OfertaId { get; set; }
    public Oferta? Oferta { get; set; }
    [Required, StringLength(80)] public string Motivo { get; set; } = string.Empty;
    [StringLength(1000)] public string? Detalle { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public EstadoReporte Estado { get; set; } = EstadoReporte.Pendiente;
}

public enum EstadoReporte { Pendiente = 0, EnRevision = 1, Resuelto = 2, Descartado = 3 }