namespace ProgIIICazador.Web.Models;

public class Oferta
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Beneficio { get; set; } = string.Empty;
    public string Condiciones { get; set; } = string.Empty;
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public string UrlFuente { get; set; } = string.Empty;
    public DateTime FechaCaptura { get; set; } = DateTime.UtcNow;
    public DateTime UltimaVerificacion { get; set; } = DateTime.UtcNow;
    public EstadoOferta Estado { get; set; } = EstadoOferta.PendienteRevision;
    public string? HuellaContenido { get; set; }
    public ICollection<Reporte> Reportes { get; set; } = new List<Reporte>();
}

public enum EstadoOferta
{
    PendienteRevision = 0,
    Publicada = 1,
    Vencida = 2,
    Observada = 3,
    Inactiva = 4
}