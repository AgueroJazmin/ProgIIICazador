using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProgIIICazador.Web.Data;

#nullable disable

namespace ProgIIICazador.Web.Migrations;

[DbContext(typeof(OfertasDbContext))]
[Migration("20261112010000_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Empresas",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Slug = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                UrlOficial = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                Activa = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Empresas", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Ofertas",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                EmpresaId = table.Column<int>(type: "int", nullable: false),
                Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                Descripcion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                Beneficio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Condiciones = table.Column<string>(type: "nvarchar(max)", maxLength: 6000, nullable: false),
                VigenciaDesde = table.Column<DateTime>(type: "datetime2", nullable: true),
                VigenciaHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                UrlFuente = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                FechaCaptura = table.Column<DateTime>(type: "datetime2", nullable: false),
                UltimaVerificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                Estado = table.Column<int>(type: "int", nullable: false),
                HuellaContenido = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Ofertas", x => x.Id);
                table.ForeignKey("FK_Ofertas_Empresas_EmpresaId", x => x.EmpresaId, "Empresas", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CapturasFuente",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                EmpresaId = table.Column<int>(type: "int", nullable: false),
                Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                TituloDetectado = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                TextoDetectado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                DatosEstructuradosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                FechaCapturaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                Estado = table.Column<int>(type: "int", nullable: false),
                HuellaContenido = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                OfertaId = table.Column<int>(type: "int", nullable: true),
                EsOfertaDetectada = table.Column<bool>(type: "bit", nullable: false),
                ConfianzaClasificacion = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CapturasFuente", x => x.Id);
                table.ForeignKey("FK_CapturasFuente_Empresas_EmpresaId", x => x.EmpresaId, "Empresas", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_CapturasFuente_Ofertas_OfertaId", x => x.OfertaId, "Ofertas", "Id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "Beneficios",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                OfertaId = table.Column<int>(type: "int", nullable: false),
                Tipo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Porcentaje = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                Importe = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                Moneda = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                TopeReintegro = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                MedioDePago = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Beneficios", x => x.Id);
                table.ForeignKey("FK_Beneficios_Ofertas_OfertaId", x => x.OfertaId, "Ofertas", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Reportes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                OfertaId = table.Column<int>(type: "int", nullable: false),
                Motivo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Detalle = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                Estado = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reportes", x => x.Id);
                table.ForeignKey("FK_Reportes_Ofertas_OfertaId", x => x.OfertaId, "Ofertas", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql("""
            SET IDENTITY_INSERT [Empresas] ON;
            INSERT INTO [Empresas] ([Id], [Activa], [Nombre], [Slug], [UrlOficial]) VALUES
                (1, 1, N'McDonald''s', N'mcdonalds', N'https://www.mcdonalds.com.ar'),
                (2, 1, N'Burger King', N'burger-king', N'https://www.burgerking.com.ar/es/terminos-y-condiciones'),
                (3, 1, N'Mostaza', N'mostaza', N'https://www.mostazaweb.com.ar/bases-y-condiciones-promos/');
            SET IDENTITY_INSERT [Empresas] OFF;
            """);

        migrationBuilder.CreateIndex("IX_Empresas_Slug", "Empresas", "Slug", unique: true);
        migrationBuilder.CreateIndex("IX_Ofertas_EmpresaId", "Ofertas", "EmpresaId");
        migrationBuilder.CreateIndex("IX_Ofertas_HuellaContenido", "Ofertas", "HuellaContenido", unique: true, filter: "[HuellaContenido] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_CapturasFuente_EmpresaId", "CapturasFuente", "EmpresaId");
        migrationBuilder.CreateIndex("IX_CapturasFuente_OfertaId", "CapturasFuente", "OfertaId");
        migrationBuilder.CreateIndex("IX_CapturasFuente_HuellaContenido", "CapturasFuente", "HuellaContenido", unique: true, filter: "[HuellaContenido] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_Beneficios_OfertaId", "Beneficios", "OfertaId");
        migrationBuilder.CreateIndex("IX_Reportes_OfertaId", "Reportes", "OfertaId");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        new OfertasDbContextModelSnapshot().BuildModelForMigration(modelBuilder);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Beneficios");
        migrationBuilder.DropTable("CapturasFuente");
        migrationBuilder.DropTable("Reportes");
        migrationBuilder.DropTable("Ofertas");
        migrationBuilder.DropTable("Empresas");
    }
}
