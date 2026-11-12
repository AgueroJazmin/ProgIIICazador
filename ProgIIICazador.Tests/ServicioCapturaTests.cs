using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Models;
using ProgIIICazador.Web.Services.Captura;
using ProgIIICazador.Web.Services.Procesamiento;
using Xunit;

namespace ProgIIICazador.Tests;

public class ServicioCapturaTests
{
    [Fact]
    public async Task EvitaGuardarDosVecesLaMismaCapturaYMarcaOfertasVencidas()
    {
        var options = new DbContextOptionsBuilder<OfertasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new OfertasDbContext(options);
        var empresa = new Empresa
        {
            Id = 1, Nombre = "Marca de prueba", Slug = "marca-prueba",
            UrlOficial = "https://oficial.example/promos", Activa = true
        };
        db.Empresas.Add(empresa);
        db.Ofertas.Add(new Oferta
        {
            EmpresaId = empresa.Id, Titulo = "Oferta anterior", UrlFuente = "https://oficial.example/anterior",
            VigenciaHasta = DateTime.Today.AddDays(-1), Estado = EstadoOferta.Publicada
        });
        await db.SaveChangesAsync();

        var extractor = new ExtractorDuplicados();
        var clasificador = new ClasificadorFijo();
        var servicio = new ServicioCaptura(db, extractor, clasificador, NullLogger<ServicioCaptura>.Instance);

        var first = await servicio.EjecutarAsync(CancellationToken.None);
        var second = await servicio.EjecutarAsync(CancellationToken.None);

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Equal(1, await db.CapturasFuente.CountAsync());
        Assert.Equal(1, clasificador.Llamadas);
        Assert.Equal(EstadoOferta.Vencida, (await db.Ofertas.SingleAsync()).Estado);
    }

    private sealed class ExtractorDuplicados : IExtractorFuente
    {
        public Task<IReadOnlyCollection<ResultadoExtraccion>> ExtraerAsync(Uri url, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<ResultadoExtraccion> results =
            [
                new(url.ToString(), "Promo", "25% de descuento", null, "hash-igual"),
                new(url.ToString(), "Promo duplicada", "25% de descuento", null, "hash-igual")
            ];
            return Task.FromResult(results);
        }
    }

    private sealed class ClasificadorFijo : IClasificadorOfertas
    {
        public int Llamadas { get; private set; }

        public Task<ResultadoClasificacion> ClasificarAsync(string textoFuente, CancellationToken cancellationToken)
        {
            Llamadas++;
            return Task.FromResult(new ResultadoClasificacion(
                true, "Promo", "25% de descuento", "Condiciones", null, null,
                25, null, "Porcentaje", 0.8m, null));
        }
    }
}
