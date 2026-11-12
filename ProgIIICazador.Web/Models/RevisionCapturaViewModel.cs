using System.ComponentModel.DataAnnotations;

namespace ProgIIICazador.Web.Models;

public class RevisionCapturaViewModel
{
    [Required] public int CapturaId { get; set; }
    [StringLength(180)] public string Titulo { get; set; } = string.Empty;
    [StringLength(4000)] public string Descripcion { get; set; } = string.Empty;
    [StringLength(1000)] public string Beneficio { get; set; } = string.Empty;
    [StringLength(6000)] public string Condiciones { get; set; } = string.Empty;
    [StringLength(80)] public string TipoBeneficio { get; set; } = "Beneficio";
    [StringLength(3000)] public string? BeneficiosAdicionales { get; set; }
    [Range(0, 100)] public decimal? Porcentaje { get; set; }
    [Range(0, 100000000)] public decimal? Importe { get; set; }
    [Range(0, 100000000)] public decimal? TopeReintegro { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    [Required, Url] public string UrlFuente { get; set; } = string.Empty;
    public bool Publicar { get; set; }
}