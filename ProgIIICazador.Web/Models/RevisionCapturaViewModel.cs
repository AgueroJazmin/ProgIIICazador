using System.ComponentModel.DataAnnotations;

namespace ProgIIICazador.Web.Models;

public class RevisionCapturaViewModel
{
    [Required] public int CapturaId { get; set; }
    [Required, StringLength(180)] public string Titulo { get; set; } = string.Empty;
    [StringLength(4000)] public string Descripcion { get; set; } = string.Empty;
    [Required, StringLength(1000)] public string Beneficio { get; set; } = string.Empty;
    [StringLength(6000)] public string Condiciones { get; set; } = string.Empty;
    [StringLength(80)] public string TipoBeneficio { get; set; } = "Beneficio";
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    [Required, Url] public string UrlFuente { get; set; } = string.Empty;
    public bool Publicar { get; set; }
}