using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProgIIICazador.Web.Services.Captura;

/// <summary>Extrae metadatos y ofertas Schema.org sin ejecutar JavaScript ni eludir controles del sitio.</summary>
public sealed class ExtractorWebOficial(HttpClient http) : IExtractorFuente
{
    public async Task<IReadOnlyCollection<ResultadoExtraccion>> ExtraerAsync(Uri url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("CazadorDeOfertasBot/1.0 (+contacto del administrador)");
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode == 429)
            throw new HttpRequestException("La fuente solicitó reducir la frecuencia de consultas.");
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        var title = Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline)
            .Groups[1].Value;
        title = WebUtility.HtmlDecode(Regex.Replace(title, @"\s+", " ")).Trim();
        var text = WebUtility.HtmlDecode(Regex.Replace(Regex.Replace(html, @"<(script|style|noscript)[^>]*>.*?</\1>", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline), "<[^>]+>", " "));
        text = Regex.Replace(text, @"\s+", " ").Trim();
        var results = new List<ResultadoExtraccion>();
        foreach (Match match in Regex.Matches(html, @"<script[^>]*type=[""']application/ld\+json[""'][^>]*>(.*?)</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            var json = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
            try
            {
                using var document = JsonDocument.Parse(json);
                foreach (var item in Flatten(document.RootElement))
                {
                    if (!LooksLikeOffer(item)) continue;
                    var name = ReadString(item, "name") ?? title;
                    var description = ReadString(item, "description") ?? string.Empty;
                    var candidateText = string.Join(" ", new[] { name, description }.Where(x => !string.IsNullOrWhiteSpace(x)));
                    var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url + candidateText)));
                    results.Add(new ResultadoExtraccion(url.ToString(), name, candidateText, item.GetRawText(), fingerprint));
                }
            }
            catch (JsonException) { /* Un bloque estructurado inválido no invalida el resto de la página. */ }
        }
        if (results.Count == 0 && !string.IsNullOrWhiteSpace(text))
        {
            var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url + text)));
            results.Add(new ResultadoExtraccion(url.ToString(), title, text[..Math.Min(text.Length, 12000)], null, fingerprint));
        }
        return results;
    }

    private static IEnumerable<JsonElement> Flatten(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
            foreach (var child in element.EnumerateArray())
                foreach (var nested in Flatten(child)) yield return nested;
        else if (element.ValueKind == JsonValueKind.Object)
        {
            yield return element;
            if (element.TryGetProperty("@graph", out var graph))
                foreach (var nested in Flatten(graph)) yield return nested;
            if (element.TryGetProperty("itemListElement", out var list))
                foreach (var nested in Flatten(list)) yield return nested;
        }
    }

    private static bool LooksLikeOffer(JsonElement item)
    {
        if (!item.TryGetProperty("@type", out var type)) return false;
        var values = type.ValueKind == JsonValueKind.Array ? type.EnumerateArray().Select(x => x.ToString()) : new[] { type.ToString() };
        return values.Any(x => x.Equals("Offer", StringComparison.OrdinalIgnoreCase) || x.Equals("AggregateOffer", StringComparison.OrdinalIgnoreCase));
    }

    private static string? ReadString(JsonElement item, string property) =>
        item.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}