using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ProgIIICazador.Web.Models;

#nullable disable

namespace ProgIIICazador.Web.Migrations;

[DbContext(typeof(ProgIIICazador.Web.Data.OfertasDbContext))]
public sealed class OfertasDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.ToTable("Empresas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("int");
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(120).HasColumnType("nvarchar(120)");
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            entity.Property(e => e.UrlOficial).HasMaxLength(2048).HasColumnType("nvarchar(2048)");
            entity.Property(e => e.Activa).HasColumnType("bit");
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasData(
                new Empresa { Id = 1, Nombre = "McDonald's", Slug = "mcdonalds", UrlOficial = "https://www.mcdonalds.com.ar", Activa = true },
                new Empresa { Id = 2, Nombre = "Burger King", Slug = "burger-king", UrlOficial = "https://www.burgerking.com.ar/es/terminos-y-condiciones", Activa = true },
                new Empresa { Id = 3, Nombre = "Mostaza", Slug = "mostaza", UrlOficial = "https://www.mostazaweb.com.ar/bases-y-condiciones-promos/", Activa = true });
        });

        modelBuilder.Entity<Oferta>(entity =>
        {
            entity.ToTable("Ofertas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("int");
            entity.Property(e => e.EmpresaId).HasColumnType("int");
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(180).HasColumnType("nvarchar(180)");
            entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(4000).HasColumnType("nvarchar(4000)");
            entity.Property(e => e.Beneficio).IsRequired().HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            entity.Property(e => e.Condiciones).IsRequired().HasMaxLength(6000).HasColumnType("nvarchar(6000)");
            entity.Property(e => e.VigenciaDesde).HasColumnType("datetime2");
            entity.Property(e => e.VigenciaHasta).HasColumnType("datetime2");
            entity.Property(e => e.UrlFuente).IsRequired().HasMaxLength(2048).HasColumnType("nvarchar(2048)");
            entity.Property(e => e.FechaCaptura).HasColumnType("datetime2");
            entity.Property(e => e.UltimaVerificacion).HasColumnType("datetime2");
            entity.Property(e => e.Estado).HasColumnType("int");
            entity.Property(e => e.HuellaContenido).HasMaxLength(64).HasColumnType("nvarchar(64)");
            entity.HasIndex(e => e.EmpresaId);
            entity.HasIndex(e => e.HuellaContenido).IsUnique().HasFilter("[HuellaContenido] IS NOT NULL");
            entity.HasOne(e => e.Empresa).WithMany(e => e.Ofertas).HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict).IsRequired();
        });

        modelBuilder.Entity<Beneficio>(entity =>
        {
            entity.ToTable("Beneficios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("int");
            entity.Property(e => e.OfertaId).HasColumnType("int");
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
            entity.Property(e => e.Porcentaje).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Importe).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Moneda).HasMaxLength(12).HasColumnType("nvarchar(12)");
            entity.Property(e => e.TopeReintegro).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MedioDePago).HasMaxLength(500).HasColumnType("nvarchar(500)");
            entity.HasIndex(e => e.OfertaId);
            entity.HasOne(e => e.Oferta).WithMany(e => e.Beneficios).HasForeignKey(e => e.OfertaId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        });

        modelBuilder.Entity<Reporte>(entity =>
        {
            entity.ToTable("Reportes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("int");
            entity.Property(e => e.OfertaId).HasColumnType("int");
            entity.Property(e => e.Motivo).IsRequired().HasMaxLength(80).HasColumnType("nvarchar(80)");
            entity.Property(e => e.Detalle).HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime2");
            entity.Property(e => e.Estado).HasColumnType("int");
            entity.HasIndex(e => e.OfertaId);
            entity.HasOne(e => e.Oferta).WithMany(e => e.Reportes).HasForeignKey(e => e.OfertaId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        });

        modelBuilder.Entity<CapturaFuente>(entity =>
        {
            entity.ToTable("CapturasFuente");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("int");
            entity.Property(e => e.EmpresaId).HasColumnType("int");
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2048).HasColumnType("nvarchar(2048)");
            entity.Property(e => e.TituloDetectado).HasMaxLength(300).HasColumnType("nvarchar(300)");
            entity.Property(e => e.TextoDetectado).IsRequired().HasColumnType("nvarchar(max)");
            entity.Property(e => e.DatosEstructuradosJson).HasColumnType("nvarchar(max)");
            entity.Property(e => e.FechaCapturaUtc).HasColumnType("datetime2");
            entity.Property(e => e.Estado).HasColumnType("int");
            entity.Property(e => e.HuellaContenido).HasMaxLength(64).HasColumnType("nvarchar(64)");
            entity.Property(e => e.OfertaId).HasColumnType("int");
            entity.Property(e => e.EsOfertaDetectada).HasColumnType("bit");
            entity.Property(e => e.ConfianzaClasificacion).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Observacion).HasColumnType("nvarchar(max)");
            entity.HasIndex(e => e.EmpresaId);
            entity.HasIndex(e => e.OfertaId);
            entity.HasIndex(e => e.HuellaContenido).IsUnique().HasFilter("[HuellaContenido] IS NOT NULL");
            entity.HasOne(e => e.Empresa).WithMany().HasForeignKey(e => e.EmpresaId).OnDelete(DeleteBehavior.Restrict).IsRequired();
            entity.HasOne(e => e.Oferta).WithMany().HasForeignKey(e => e.OfertaId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
