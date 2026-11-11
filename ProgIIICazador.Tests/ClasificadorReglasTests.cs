using ProgIIICazador.Web.Services.Procesamiento;
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
    public async Task NoClasificaTextoComunComoOferta()
    {
        var result = await _clasificador.ClasificarAsync("Conocé nuestros locales y horarios de atención.", CancellationToken.None);
        Assert.False(result.EsOferta);
        Assert.Null(result.TipoBeneficio);
    }
}