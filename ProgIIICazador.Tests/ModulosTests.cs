using Microsoft.Extensions.DependencyInjection;
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
        services.AddProcesamientoOfertasModule();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<ClasificadorReglas>(provider.GetRequiredService<IClasificadorOfertas>());
        Assert.IsType<OcrNoConfigurado>(provider.GetRequiredService<IProveedorOCR>());
    }
}
