using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;

namespace ProgIIICazador.Web.Services.Captura;

public sealed class CapturaProgramadaService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<CapturaProgramadaService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var minutes = Math.Clamp(configuration.GetValue("Captura:IntervaloMinutos", 360), 30, 1440);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ServicioCaptura>();
                var count = await service.EjecutarAsync(stoppingToken);
                logger.LogInformation("Captura finalizada. Nuevos candidatos: {Count}", count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Error en la ejecución programada de captura"); }
            await Task.Delay(TimeSpan.FromMinutes(minutes), stoppingToken);
        }
    }
}