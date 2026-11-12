using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ProgIIICazador.Web.Modules.GestionOfertas;
using ProgIIICazador.Web.Services.Procesamiento;
using Xunit;

namespace ProgIIICazador.Tests;

public class ModulosTests
{
    [Fact]
    public void ProcesamientoUsaProveedoresLocalesCuandoNoHayCredenciales()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddProcesamientoOfertasModule();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<ClasificadorReglas>(provider.GetRequiredService<IClasificadorOfertas>());
        Assert.IsType<OcrNoConfigurado>(provider.GetRequiredService<IProveedorOCR>());
    }
}
