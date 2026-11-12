using ProgIIICazador.Web.Services.Procesamiento;
using ProgIIICazador.Web.Services.Captura;
using Xunit;

namespace ProgIIICazador.Tests;

public class ClasificadorReglasTests
{
    private readonly ClasificadorReglas _clasificador = new();

    [Fact]
    public async Task DetectaPorcentajeDeDescuento()
    {
        var result = await _clasificador.ClasificarAsync("20% de descuento en productos seleccionados", CancellationToken.None);
        Assert.True(result.EsOferta);
        Assert.Equal(20m, result.Porcentaje);
        Assert.Equal("Porcentaje", result.TipoBeneficio);
    }

    [Fact]
    public async Task DetectaImporteEnPesos()
    {
        var result = await _clasificador.ClasificarAsync("Reintegro con tope de $15.000 por mes", CancellationToken.None);
        Assert.True(result.EsOferta);
        Assert.Equal(15000m, result.Importe);
    }

    [Fact]
    public async Task NoClasificaUnPrecioNormalComoPromocion()
    {
        var result = await _clasificador.ClasificarAsync("Combo clásico a $5.000", CancellationToken.None);
        Assert.False(result.EsOferta);
        Assert.Null(result.TipoBeneficio);
    }

    [Fact]
    public async Task NoClasificaTextoComunComoOferta()
    {
        var result = await _clasificador.ClasificarAsync("Conocé nuestros locales y horarios de atención.", CancellationToken.None);
        Assert.False(result.EsOferta);
        Assert.Null(result.TipoBeneficio);
    }
    [Fact]
    public void SeparaVariasPromocionesPublicadasEnUnaMismaPagina()
    {
        const string html = """
            <html><body>
            <h2>PROMOCIÓN «REINTEGRO BANCO SANTANDER»</h2>
            <p>Beneficio del 25% de reintegro con tope mensual de $15.000.</p>
            <h2>PROMOCIÓN «BANCO MACRO – 30% DE AHORRO»</h2>
            <p>30% de ahorro con tope de $30.000 mensuales.</p>
            </body></html>
            """;

        var sections = SeparadorSeccionesPromocionales.Separar(html);

        Assert.Equal(2, sections.Count);
        Assert.Contains("SANTANDER", sections[0].Titulo);
        Assert.Contains("25%", sections[0].Texto);
        Assert.Contains("MACRO", sections[1].Titulo);
        Assert.Contains("30%", sections[1].Texto);
    }

    [Fact]
    public void IgnoraBasesDeUnConcursoSinBeneficioDeDescuento()
    {
        const string html = """
            <html><body>
            <h2>PROMOCIÓN «PARTICIPÁ POR UN VIAJE»</h2>
            <p>Participá del sorteo de un viaje. Se seleccionará un ganador.</p>
            <h2>PROMOCIÓN «REINTEGRO BANCO»</h2>
            <p>25% de reintegro con tope mensual de $10.000.</p>
            </body></html>
            """;

        var sections = SeparadorSeccionesPromocionales.Separar(html);

        Assert.Single(sections);
        Assert.Contains("REINTEGRO", sections[0].Titulo);
    }

    [Fact]
    public void NoInventaSeccionesCuandoLaPaginaNoTieneEncabezadosDePromocion()
    {
        var sections = SeparadorSeccionesPromocionales.Separar(
            "<html><body><h1>Locales y horarios</h1><p>Atención todos los días.</p></body></html>");

        Assert.Empty(sections);
    }

}