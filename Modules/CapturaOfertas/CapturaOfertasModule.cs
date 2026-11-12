using ProgIIICazador.Web.Services.Captura;

namespace ProgIIICazador.Web.Modules.CapturaOfertas;

/// <summary>Registra los componentes responsables de consultar fuentes oficiales y programar capturas.</summary>
public static class CapturaOfertasModule
{
    public static IServiceCollection AddCapturaOfertasModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IExtractorFuente, ExtractorWebOficial>(client =>
            client.Timeout = TimeSpan.FromSeconds(25));
        services.AddScoped<ServicioCaptura>();

        if (configuration.GetValue<bool>("Captura:Habilitada"))
            services.AddHostedService<CapturaProgramadaService>();

        return services;
    }
}
