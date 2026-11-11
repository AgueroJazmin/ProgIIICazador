using System.ComponentModel.DataAnnotations;

namespace ProgIIICazador.Web.Models;

public class Oferta
{
    public int Id { get; set; }
    [Required] public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    [Required, StringLength(180)] public string Titulo { get; set; } = string.Empty;
    [StringLength(4000)] public string Descripcion { get; set; } = string.Empty;
    [StringLength(1000)] public string Beneficio { get; set; } = string.Empty;
    [StringLength(6000)] public string Condiciones { get; set; } = string.Empty;
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    [Required, Url, StringLength(2048)] public string UrlFuente { get; set; } = string.Empty;
    public DateTime FechaCaptura { get; set; } = DateTime.UtcNow;
    public DateTime UltimaVerificacion { get; set; } = DateTime.UtcNow;
    public EstadoOferta Estado { get; set; } = EstadoOferta.PendienteRevision;
    [StringLength(64)] public string? HuellaContenido { get; set; }
    public ICollection<Beneficio> Beneficios { get; set; } = new List<Beneficio>();
    public ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}

public enum EstadoOferta { PendienteRevision = 0, Publicada = 1, Vencida = 2, Observada = 3, Inactiva = 4 }