using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Models;

namespace ProgIIICazador.Web.Data;

public class OfertasDbContext(DbContextOptions<OfertasDbContext> options) : DbContext(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Oferta> Ofertas => Set<Oferta>();
    public DbSet<Reporte> Reportes => Set<Reporte>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Empresa>()
            .HasIndex(e => e.Slug).IsUnique();

        modelBuilder.Entity<Empresa>().HasData(
            new Empresa { Id = 1, Nombre = "McDonald's", Slug = "mcdonalds", UrlOficial = "https://www.mcdonalds.com.ar", Activa = true },
            new Empresa { Id = 2, Nombre = "Burger King", Slug = "burger-king", UrlOficial = "https://www.burgerking.com.ar", Activa = true },
            new Empresa { Id = 3, Nombre = "Mostaza", Slug = "mostaza", UrlOficial = "https://www.mostazaweb.com.ar", Activa = true }
        );

        modelBuilder.Entity<Oferta>()
            .HasOne(o => o.Empresa)
            .WithMany(e => e.Ofertas)
            .HasForeignKey(o => o.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reporte>()
            .HasOne(r => r.Oferta)
            .WithMany(o => o.Reportes)
            .HasForeignKey(r => r.OfertaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}