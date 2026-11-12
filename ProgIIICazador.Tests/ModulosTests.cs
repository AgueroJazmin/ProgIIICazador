using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Models;
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
    [Fact]
    public void LasColumnasIndexadasTienenLongitudCompatibleConSqlServer()
    {
        var options = new DbContextOptionsBuilder<OfertasDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ModeloTest;Trusted_Connection=True")
            .Options;
        using var db = new OfertasDbContext(options);

        Assert.Equal(80, db.Model.FindEntityType(typeof(Empresa))!.FindProperty(nameof(Empresa.Slug))!.GetMaxLength());
        Assert.Equal(64, db.Model.FindEntityType(typeof(Oferta))!.FindProperty(nameof(Oferta.HuellaContenido))!.GetMaxLength());
        Assert.Equal(64, db.Model.FindEntityType(typeof(CapturaFuente))!.FindProperty(nameof(CapturaFuente.HuellaContenido))!.GetMaxLength());
    }

    [Fact]
    public void LaMigracionInicialEstaDisponibleParaEFCore()
    {
        var options = new DbContextOptionsBuilder<OfertasDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ModeloTest;Trusted_Connection=True")
            .Options;
        using var db = new OfertasDbContext(options);

        Assert.Contains("20261112010000_InitialCreate", db.Database.GetMigrations());
        var script = db.Database.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>().GenerateScript();
        Assert.Contains("CREATE TABLE [Empresas]", script);
        Assert.Contains("CREATE TABLE [Ofertas]", script);
        Assert.Contains("CREATE TABLE [CapturasFuente]", script);
    }

}
