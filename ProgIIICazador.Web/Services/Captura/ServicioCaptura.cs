using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Models;

namespace ProgIIICazador.Web.Services.Captura;

public sealed class ServicioCaptura(OfertasDbContext db, IExtractorFuente extractor, ILogger<ServicioCaptura> logger)
{
    public async Task<int> EjecutarAsync(CancellationToken cancellationToken)
    {
        var empresas = await db.Empresas.Where(e => e.Activa && e.UrlOficial != null).ToListAsync(cancellationToken);
        var nuevas = 0;
        foreach (var empresa in empresas)
        {
            if (!Uri.TryCreate(empresa.UrlOficial, UriKind.Absolute, out var url) || url.Scheme != Uri.UriSchemeHttps)
            {
                logger.LogWarning("URL oficial inválida para {Empresa}", empresa.Nombre);
                continue;
            }
            try
            {
                var resultados = await extractor.ExtraerAsync(url, cancellationToken);
                foreach (var resultado in resultados)
                {
                    if (await db.CapturasFuente.AnyAsync(c => c.HuellaContenido == resultado.Huella, cancellationToken))
                        continue;
                    db.CapturasFuente.Add(new CapturaFuente
                    {
                        EmpresaId = empresa.Id, Url = resultado.Url, TituloDetectado = resultado.Titulo,
                        TextoDetectado = resultado.Texto, DatosEstructuradosJson = resultado.DatosJson,
                        HuellaContenido = resultado.Huella, Estado = EstadoCaptura.PendienteRevision
                    });
                    nuevas++;
                }
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                logger.LogWarning(ex, "No se pudo consultar la fuente de {Empresa}", empresa.Nombre);
            }
        }
        return nuevas;
    }
}