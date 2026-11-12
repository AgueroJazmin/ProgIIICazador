namespace ProgIIICazador.Web.Services.Captura;

public sealed record ResultadoExtraccion(string Url, string? Titulo, string Texto, string? DatosJson, string Huella);