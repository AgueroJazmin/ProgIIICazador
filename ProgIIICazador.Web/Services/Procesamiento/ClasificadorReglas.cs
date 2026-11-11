using System.Globalization;
using System.Text.RegularExpressions;

namespace ProgIIICazador.Web.Services.Procesamiento;

/// <summary>Clasificador conservador de respaldo. Los resultados requieren revisión humana.</summary>
public sealed class ClasificadorReglas : IClasificadorOfertas
{
    public Task<ResultadoClasificacion> ClasificarAsync(string textoFuente, CancellationToken cancellationToken)
    {
        var text = textoFuente ?? string.Empty;
        var percent = Regex.Match(text, @"(?<!\d)(\d{1,2})\s*%");
        decimal? percentage = percent.Success && decimal.TryParse(percent.Groups[1].Value, out var p) ? p : null;
        var money = Regex.Match(text, @"\$\s*([\d.]+(?:,\d{1,2})?)");
        decimal? amount = null;
        if (money.Success && decimal.TryParse(money.Groups[1].Value.Replace(".", "").Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out var a)) amount = a;
        var isOffer = percentage.HasValue || amount.HasValue || Regex.IsMatch(text, @"\b(beneficio|descuento|reintegro|promo|promoción|cupón)\b", RegexOptions.IgnoreCase);
        var type = percentage.HasValue ? "Porcentaje" : amount.HasValue ? "Importe" : isOffer ? "Beneficio" : null;
        return Task.FromResult(new ResultadoClasificacion(isOffer, null, null, text.Length > 3000 ? text[..3000] : text, null, null, percentage, amount, type, isOffer ? 0.35m : 0.10m, "Clasificación automática preliminar; verificar contra la fuente oficial."));
    }
}