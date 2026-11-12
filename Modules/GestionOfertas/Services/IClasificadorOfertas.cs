namespace ProgIIICazador.Web.Modules.GestionOfertas.Services;

public interface IClasificadorOfertas
{
    Task<ResultadoClasificacion> ClasificarAsync(string textoFuente, CancellationToken cancellationToken);
}

public sealed record ResultadoClasificacion(bool EsOferta, string? Titulo, string? Beneficio, string? Condiciones, DateTime? Desde, DateTime? Hasta, decimal? Porcentaje, decimal? Importe, string? TipoBeneficio, decimal Confianza, string? Observaciones);