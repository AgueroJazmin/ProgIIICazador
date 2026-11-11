using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ProgIIICazador.Web.Services.Procesamiento;

/// <summary>Conector opcional con una API compatible con OpenAI. Toda salida queda sujeta a revisión.</summary>
public sealed class ClasificadorIA(HttpClient http, IConfiguration configuration) : IClasificadorOfertas
{
    public async Task<ResultadoClasificacion> ClasificarAsync(string textoFuente, CancellationToken cancellationToken)
    {
        var endpoint = configuration["IA:Endpoint"]?.TrimEnd('/');
        var key = configuration["IA:ApiKey"];
        var model = configuration["IA:Modelo"];
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(model))
            throw new InvalidOperationException("Configurá IA:Endpoint, IA:ApiKey e IA:Modelo para habilitar la clasificación con IA.");
        var prompt = """
        Analizá el texto de una publicación de una cadena de comida rápida de Argentina.
        Extraé únicamente datos explícitos. No inventes porcentajes, importes, fechas, requisitos ni condiciones.
        Respondé SOLO JSON válido con estas propiedades:
        esOferta (boolean), titulo (string|null), beneficio (string|null), condiciones (string|null),
        desde (string|null formato yyyy-MM-dd), hasta (string|null formato yyyy-MM-dd),
        porcentaje (number|null), importe (number|null), tipoBeneficio (string|null),
        confianza (number entre 0 y 1), observaciones (string|null).
        Si no hay evidencia clara de una promoción, esOferta=false. No confundas precio normal con descuento.
        """;
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint + "/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        request.Content = new StringContent(JsonSerializer.Serialize(new
        {
            model,
            temperature = 0,
            response_format = new { type = "json_object" },
            messages = new[] { new { role = "system", content = prompt }, new { role = "user", content = textoFuente[..Math.Min(textoFuente.Length, 12000)] } }
        }), Encoding.UTF8, "application/json");
        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()
            ?? throw new InvalidOperationException("La IA no devolvió contenido.");
        using var result = JsonDocument.Parse(content);
        var root = result.RootElement;
        string? S(string name) => root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;
        decimal? N(string name) => root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetDecimal(out var n) ? n : null;
        DateTime? D(string name) => DateTime.TryParseExact(S(name), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
        var isOffer = root.TryGetProperty("esOferta", out var offer) && offer.ValueKind == JsonValueKind.True;
        var confidence = N("confianza") ?? 0;
        return new ResultadoClasificacion(isOffer, S("titulo"), S("beneficio"), S("condiciones"), D("desde"), D("hasta"),
            N("porcentaje"), N("importe"), S("tipoBeneficio"), Math.Clamp(confidence, 0, 1), S("observaciones"));
    }
}