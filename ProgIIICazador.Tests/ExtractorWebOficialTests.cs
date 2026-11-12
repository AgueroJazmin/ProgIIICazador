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

    [Fact]
    public async Task BloqueaRedireccionesFueraDelDominioOriginal()
    {
        var extractor = new ExtractorWebOficial(new HttpClient(new DelegadoHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Redirect)
            {
                Headers = { Location = new Uri("https://sitio-ajeno.example/robo") }
            })));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            extractor.ExtraerAsync(new Uri("https://oficial.example/promos"), CancellationToken.None));
    }

    [Fact]
    public async Task BloqueaRespuestasQueSuperanElLimiteDeTamano()
    {
        var extractor = new ExtractorWebOficial(new HttpClient(new DelegadoHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(new string('x', 2 * 1024 * 1024 + 1))
            })));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            extractor.ExtraerAsync(new Uri("https://oficial.example/promos"), CancellationToken.None));
    }

    [Fact]
    public async Task SigueRedireccionesDentroDelMismoDominio()
    {
        var requests = 0;
        var extractor = new ExtractorWebOficial(new HttpClient(new DelegadoHandler(_ =>
        {
            requests++;
            return requests == 1
                ? new HttpResponseMessage(HttpStatusCode.Redirect)
                {
                    Headers = { Location = new Uri("/destino", UriKind.Relative) }
                }
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<html><head><title>Menú</title></head><body>Sin promociones.</body></html>")
                };
        })));

        var results = await extractor.ExtraerAsync(new Uri("https://oficial.example/promos"), CancellationToken.None);

        Assert.Empty(results);
        Assert.Equal(2, requests);
    }

    private static ExtractorWebOficial CrearExtractor(string html) =>
        new(new HttpClient(new DelegadoHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html) })));

    private sealed class DelegadoHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }
}
