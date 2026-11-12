using ProgIIICazador.Web.Modules.CapturaOfertas.Extractors;
using ProgIIICazador.Web.Modules.CapturaOfertas.Services;

namespace ProgIIICazador.Web.Modules.CapturaOfertas;

/// <summary>Registra los componentes responsables de consultar fuentes oficiales y programar capturas.</summary>
public static class CapturaOfertasModule
{
    public static IServiceCollection AddCapturaOfertasModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IExtractorFuente, ExtractorWebOficial>(client =>
            client.Timeout = TimeSpan.FromSeconds(25))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // Las redirecciones se validan en el extractor para impedir salir del dominio oficial.
                AllowAutoRedirect = false
            });
        services.AddScoped<ServicioCaptura>();

        if (configuration.GetValue<bool>("Captura:Habilitada"))
            services.AddHostedService<CapturaProgramadaService>();

        return services;
    }
}
