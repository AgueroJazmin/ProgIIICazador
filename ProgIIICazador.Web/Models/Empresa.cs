namespace ProgIIICazador.Web.Models;

public class Empresa
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? UrlOficial { get; set; }
    public bool Activa { get; set; } = true;
    public ICollection<Oferta> Ofertas { get; set; } = new List<Oferta>();
}