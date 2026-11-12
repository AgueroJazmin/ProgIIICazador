using System.Net.Http.Headers;
using System.Text;

namespace ProgIIICazador.Web.Modules.GestionOfertas.Services;

public sealed class AzureDocumentIntelligenceOcr(HttpClient http, IConfiguration configuration) : IProveedorOCR
{
    public async Task<string> ExtraerTextoAsync(Stream archivo, string nombreArchivo, CancellationToken cancellationToken)
    {
        var endpoint = configuration["Ocr:Endpoint"]?.TrimEnd('/');
        var key = configuration["Ocr:Key"];
        var model = configuration["Ocr:ModelId"] ?? "prebuilt-read";
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Configurá Ocr:Endpoint y Ocr:Key para habilitar OCR.");
        using var content = new StreamContent(archivo);
        content.Headers.ContentType = new MediaTypeHeaderValue(ContentType(nombreArchivo));
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"{endpoint}/documentintelligence/documentModels/{Uri.EscapeDataString(model)}:analyze?api-version=2024-11-30") { Content = content };
        request.Headers.Add("Ocp-Apim-Subscription-Key", key);
        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (!response.Headers.TryGetValues("Operation-Location", out var locations))
            throw new InvalidOperationException("El proveedor OCR no devolvió la URL de seguimiento.");
        var operation = locations.First();
        for (var attempt = 0; attempt < 30; attempt++)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            using var poll = new HttpRequestMessage(HttpMethod.Get, operation);
            poll.Headers.Add("Ocp-Apim-Subscription-Key", key);
            using var result = await http.SendAsync(poll, cancellationToken);
            result.EnsureSuccessStatusCode();
            using var json = await System.Text.Json.JsonDocument.ParseAsync(await result.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = json.RootElement;
            var status = root.TryGetProperty("status", out var s) ? s.GetString() : null;
            if (status == "succeeded")
                return root.GetProperty("analyzeResult").GetProperty("content").GetString() ?? string.Empty;
            if (status == "failed") throw new InvalidOperationException("El proveedor no pudo leer el archivo.");
        }
        throw new TimeoutException("El OCR tardó más de lo esperado. Intentá nuevamente.");
    }

    private static string ContentType(string name) => Path.GetExtension(name).ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf", ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg",
        _ => throw new InvalidOperationException("Formato no admitido. Usá PDF, PNG o JPG.")
    };
}