using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Models;

namespace ProgIIICazador.Web.Data;

public class OfertasDbContext(DbContextOptions<OfertasDbContext> options) : DbContext(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Oferta> Ofertas => Set<Oferta>();
    public DbSet<Beneficio> Beneficios => Set<Beneficio>();
    public DbSet<Reporte> Reportes => Set<Reporte>();
    public DbSet<CapturaFuente> CapturasFuente => Set<CapturaFuente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Empresa>().HasIndex(e => e.Slug).IsUnique();
        modelBuilder.Entity<Oferta>().HasIndex(o => o.HuellaContenido).IsUnique().HasFilter("[HuellaContenido] IS NOT NULL");
        modelBuilder.Entity<CapturaFuente>().HasIndex(c => c.HuellaContenido).IsUnique().HasFilter("[HuellaContenido] IS NOT NULL");
        modelBuilder.Entity<Empresa>().HasData(
            new Empresa { Id = 1, Nombre = "McDonald's", Slug = "mcdonalds", UrlOficial = "https://www.mcdonalds.com.ar", Activa = true },
            new Empresa { Id = 2, Nombre = "Burger King", Slug = "burger-king", UrlOficial = "https://www.burgerking.com.ar/es/terminos-y-condiciones", Activa = true },
            new Empresa { Id = 3, Nombre = "Mostaza", Slug = "mostaza", UrlOficial = "https://www.mostazaweb.com.ar/bases-y-condiciones-promos/", Activa = true });
        modelBuilder.Entity<Oferta>().HasOne(o => o.Empresa).WithMany(e => e.Ofertas).HasForeignKey(o => o.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Beneficio>().HasOne(b => b.Oferta).WithMany(o => o.Beneficios).HasForeignKey(b => b.OfertaId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Reporte>().HasOne(r => r.Oferta).WithMany(o => o.Reportes).HasForeignKey(r => r.OfertaId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CapturaFuente>().HasOne(c => c.Empresa).WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CapturaFuente>().HasOne(c => c.Oferta).WithMany().HasForeignKey(c => c.OfertaId).OnDelete(DeleteBehavior.SetNull);
    }
}