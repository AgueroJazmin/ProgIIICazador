using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
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
    public void ElSnapshotDeMigracionesCoincideConElModeloActual()
    {
        var options = new DbContextOptionsBuilder<OfertasDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ModeloTest;Trusted_Connection=True")
            .Options;
        using var db = new OfertasDbContext(options);
        var snapshot = db.GetService<IMigrationsAssembly>().ModelSnapshot!;
        var initializer = db.GetService<IModelRuntimeInitializer>();
        var snapshotModel = initializer.Initialize(snapshot.Model, designTime: true);
        var differ = db.GetService<IMigrationsModelDiffer>();
        var currentModel = db.GetService<IDesignTimeModel>().Model;
        var differences = differ.GetDifferences(snapshotModel.GetRelationalModel(), currentModel.GetRelationalModel());

        Assert.True(differences.Count == 0, string.Join(Environment.NewLine, differences.Select(operation => operation.GetType().Name + ": " + operation)));
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
