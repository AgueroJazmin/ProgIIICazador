using ProgIIICazador.Web.Services.Procesamiento;

namespace ProgIIICazador.Web.Modules.GestionOfertas;

/// <summary>Registra los proveedores de OCR y clasificación automática, con alternativas locales seguras.</summary>
public static class ProcesamientoOfertasModule
{
    public static IServiceCollection AddProcesamientoOfertasModule(this IServiceCollection services)
    {
        services.AddHttpClient<AzureDocumentIntelligenceOcr>();
        services.AddScoped<IProveedorOCR>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            return string.IsNullOrWhiteSpace(configuration["Ocr:Endpoint"]) ||
                   string.IsNullOrWhiteSpace(configuration["Ocr:Key"])
                ? new OcrNoConfigurado()
                : sp.GetRequiredService<AzureDocumentIntelligenceOcr>();
        });

        services.AddHttpClient<ClasificadorIA>();
        services.AddScoped<IClasificadorOfertas>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            return string.IsNullOrWhiteSpace(configuration["IA:Endpoint"]) ||
                   string.IsNullOrWhiteSpace(configuration["IA:ApiKey"]) ||
                   string.IsNullOrWhiteSpace(configuration["IA:Modelo"])
                ? new ClasificadorReglas()
                : sp.GetRequiredService<ClasificadorIA>();
        });

        return services;
    }
}
