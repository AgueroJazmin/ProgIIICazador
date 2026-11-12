using System.Net;
using ProgIIICazador.Web.Services.Captura;
using Xunit;

namespace ProgIIICazador.Tests;

public class ExtractorWebOficialTests
{
    [Fact]
    public async Task ExtraePromocionesSeparadasDeUnaPaginaOficial()
    {
        const string html = """
            <html><head><title>Bases y condiciones</title></head><body>
            <h2>PROMOCIÓN «REINTEGRO BANCO A»</h2>
            <p>25% de reintegro con tope mensual de $10.000.</p>
            <h2>PROMOCIÓN «DESCUENTO BANCO B»</h2>
            <p>30% de descuento en compras adheridas.</p>
            </body></html>
            """;
        var extractor = CrearExtractor(html);

        var results = await extractor.ExtraerAsync(new Uri("https://oficial.example/promos"), CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, x => x.Titulo!.Contains("BANCO A"));
        Assert.Contains(results, x => x.Titulo!.Contains("BANCO B"));
    }

    [Fact]
    public async Task NoConsideraUnPrecioNormalComoOferta()
    {
        const string html = """
            <html><head><title>Menú</title></head><body>
            <h1>Nuestro menú</h1><p>Combo clásico $8.500. Hamburguesa $6.000.</p>
            </body></html>
            """;
        var extractor = CrearExtractor(html);

        var results = await extractor.ExtraerAsync(new Uri("https://oficial.example/menu"), CancellationToken.None);

        Assert.Empty(results);
    }

    private static ExtractorWebOficial CrearExtractor(string html)
    {
        var handler = new RespuestaHtmlHandler(html);
        return new ExtractorWebOficial(new HttpClient(handler));
    }

    private sealed class RespuestaHtmlHandler(string html) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html)
            });
    }
}
