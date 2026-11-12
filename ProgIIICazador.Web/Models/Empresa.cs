using System.ComponentModel.DataAnnotations;

namespace ProgIIICazador.Web.Models;

public class Empresa
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(80)] public string Slug { get; set; } = string.Empty;
    [Url, StringLength(2048)] public string? UrlOficial { get; set; }
    public bool Activa { get; set; } = true;
    public ICollection<Oferta> Ofertas { get; set; } = new List<Oferta>();
}
