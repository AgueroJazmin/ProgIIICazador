namespace ProgIIICazador.Web.Modules.CapturaOfertas.Extractors;

public sealed record ResultadoExtraccion(string Url, string? Titulo, string Texto, string? DatosJson, string Huella);