using System.ComponentModel.DataAnnotations;

namespace ProgIIICazador.Web.Models;

public class CapturaFuente
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    [Required, Url, StringLength(2048)] public string Url { get; set; } = string.Empty;
    [StringLength(300)] public string? TituloDetectado { get; set; }
    public string TextoDetectado { get; set; } = string.Empty;
    public string? DatosEstructuradosJson { get; set; }
    public DateTime FechaCapturaUtc { get; set; } = DateTime.UtcNow;
    public EstadoCaptura Estado { get; set; } = EstadoCaptura.PendienteRevision;
    [StringLength(64)] public string? HuellaContenido { get; set; }
    public int? OfertaId { get; set; }
    public Oferta? Oferta { get; set; }
    public bool EsOfertaDetectada { get; set; }
    public decimal ConfianzaClasificacion { get; set; }
    public string? Observacion { get; set; }
}

public enum EstadoCaptura { PendienteRevision = 0, Procesada = 1, Descartada = 2, Error = 3 }