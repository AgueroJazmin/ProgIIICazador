using System.ComponentModel.DataAnnotations;

namespace ProgIIICazador.Web.Models;

public class Beneficio
{
    public int Id { get; set; }
    public int OfertaId { get; set; }
    public Oferta? Oferta { get; set; }
    [Required, StringLength(80)] public string Tipo { get; set; } = "Descuento";
    [Required, StringLength(500)] public string Descripcion { get; set; } = string.Empty;
    public decimal? Porcentaje { get; set; }
    public decimal? Importe { get; set; }
    [StringLength(12)] public string? Moneda { get; set; }
    public decimal? TopeReintegro { get; set; }
    [StringLength(500)] public string? MedioDePago { get; set; }
}