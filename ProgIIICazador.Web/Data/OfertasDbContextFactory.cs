using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProgIIICazador.Web.Data;

/// <summary>Permite crear el contexto durante operaciones de diseño de EF Core.</summary>
public sealed class OfertasDbContextFactory : IDesignTimeDbContextFactory<OfertasDbContext>
{
    public OfertasDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__OfertasDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=ProgIIICazador;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<OfertasDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new OfertasDbContext(options);
    }
}
