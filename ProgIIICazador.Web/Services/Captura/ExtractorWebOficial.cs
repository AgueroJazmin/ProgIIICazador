using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ProgIIICazador.Web.Services.Captura;

/// <summary>Lee páginas HTTPS oficiales, metadatos, JSON-LD y enlaces internos relacionados con promociones.</summary>
public sealed class ExtractorWebOficial(HttpClient http) : IExtractorFuente
{
    public async Task<IReadOnlyCollection<ResultadoExtraccion>> ExtraerAsync(Uri url, CancellationToken cancellationToken)
    {
        var results = new List<ResultadoExtraccion>();
        var rootHtml = await DescargarAsync(url, cancellationToken);
        results.AddRange(AnalizarPagina(url, rootHtml));
        var links = Regex.Matches(rootHtml, @"<a\b[^>]*href=[""']([^""'#]+)[""'][^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline)
            .Select(m => new { Href = WebUtility.HtmlDecode(m.Groups[1].Value).Trim(), Text = WebUtility.HtmlDecode(Regex.Replace(m.Groups[2].Value, "<[^>]+>", " ")).Trim() })
            .Where(x => Regex.IsMatch(x.Href + " " + x.Text, @"promo|oferta|beneficio|cup[oó]n|descuento|promocion", RegexOptions.IgnoreCase))
            .Select(x => Uri.TryCreate(url, x.Href, out var target) ? target : null)
            .Where(x => x is not null && x.Scheme == Uri.UriSchemeHttps && x.Host.Equals(url.Host, StringComparison.OrdinalIgnoreCase))
            .DistinctBy(x => x!.AbsoluteUri).Take(12).ToList();

        foreach (var link in links)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (link is null || link == url) continue;
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
                var html = await DescargarAsync(link, cancellationToken);
                results.AddRange(AnalizarPagina(link, html));
            }
            catch (HttpRequestException) { /* Un enlace caído no impide procesar el resto del sitio oficial. */ }
        }
        return results;
    }

    private async Task<string> DescargarAsync(Uri url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("CazadorDeOfertasBot/1.0");
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new HttpRequestException("La fuente solicitó reducir la frecuencia de consultas.");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static IReadOnlyCollection<ResultadoExtraccion> AnalizarPagina(Uri url, string html)
    {
        var title = Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
        title = WebUtility.HtmlDecode(Regex.Replace(title, @"\s+", " ")).Trim();
        var stripped = Regex.Replace(html, @"<(script|style|noscript)[^>]*>.*?</\1>", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var text = WebUtility.HtmlDecode(Regex.Replace(stripped, "<[^>]+>", " "));
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
                    results.Add(Build(url, name, candidateText, item.GetRawText()));
                }
            }
            catch (JsonException) { }
        }
        if (results.Count == 0 && !string.IsNullOrWhiteSpace(text))
            results.Add(Build(url, title, text[..Math.Min(text.Length, 12000)], null));
        return results;
    }

    private static ResultadoExtraccion Build(Uri url, string? title, string text, string? json)
    {
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url + text)));
        return new ResultadoExtraccion(url.ToString(), title, text, json, fingerprint);
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
        IEnumerable<string> values = type.ValueKind == JsonValueKind.Array
            ? type.EnumerateArray().Select(x => x.ToString()) : new[] { type.ToString() };
        return values.Any(x => x.Equals("Offer", StringComparison.OrdinalIgnoreCase) || x.Equals("AggregateOffer", StringComparison.OrdinalIgnoreCase));
    }

    private static string? ReadString(JsonElement item, string property) =>
        item.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}