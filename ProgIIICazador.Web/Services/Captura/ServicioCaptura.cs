using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Models;
using ProgIIICazador.Web.Services.Procesamiento;

namespace ProgIIICazador.Web.Services.Captura;

public sealed class ServicioCaptura(OfertasDbContext db, IExtractorFuente extractor, IClasificadorOfertas clasificador, ILogger<ServicioCaptura> logger)
{
    public async Task<int> EjecutarAsync(CancellationToken cancellationToken)
    {
        var empresas = await db.Empresas.Where(e => e.Activa && e.UrlOficial != null).ToListAsync(cancellationToken);
        var nuevas = 0;
        foreach (var empresa in empresas)
        {
            if (!Uri.TryCreate(empresa.UrlOficial, UriKind.Absolute, out var url) || url.Scheme != Uri.UriSchemeHttps)
            { logger.LogWarning("URL oficial inválida para {Empresa}", empresa.Nombre); continue; }

            try
            {
                var resultados = await extractor.ExtraerAsync(url, cancellationToken);
                var huellasConocidas = await db.CapturasFuente
                    .Where(c => c.EmpresaId == empresa.Id && c.HuellaContenido != null)
                    .Select(c => c.HuellaContenido!)
                    .ToListAsync(cancellationToken);
                var huellas = huellasConocidas.ToHashSet(StringComparer.Ordinal);
                var nuevasEmpresa = 0;

                foreach (var resultado in resultados)
                {
                    if (string.IsNullOrWhiteSpace(resultado.Huella) || !huellas.Add(resultado.Huella))
                        continue;

                    ResultadoClasificacion clasificacion;
                    try
                    {
                        clasificacion = await clasificador.ClasificarAsync(resultado.Texto, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException or System.Text.Json.JsonException)
                    {
                        logger.LogWarning(ex, "No se pudo clasificar el contenido de {Url}", resultado.Url);
                        clasificacion = new ResultadoClasificacion(false, null, null, null, null, null, null, null, null, 0,
                            "Clasificación no disponible; revisar manualmente.");
                    }

                    db.CapturasFuente.Add(new CapturaFuente
                    {
                        EmpresaId = empresa.Id,
                        Url = resultado.Url,
                        TituloDetectado = clasificacion.Titulo ?? resultado.Titulo,
                        TextoDetectado = resultado.Texto,
                        DatosEstructuradosJson = resultado.DatosJson,
                        HuellaContenido = resultado.Huella,
                        Estado = EstadoCaptura.PendienteRevision,
                        EsOfertaDetectada = clasificacion.EsOferta,
                        ConfianzaClasificacion = clasificacion.Confianza,
                        Observacion = clasificacion.Observaciones
                    });
                    nuevasEmpresa++;
                }

                if (nuevasEmpresa > 0)
                {
                    await db.SaveChangesAsync(cancellationToken);
                    nuevas += nuevasEmpresa;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                logger.LogWarning(ex, "No se pudo consultar la fuente de {Empresa}", empresa.Nombre);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "No se pudieron guardar las capturas de {Empresa}; se continúa con las demás fuentes", empresa.Nombre);
                foreach (var entry in db.ChangeTracker.Entries<CapturaFuente>().Where(e => e.State == EntityState.Added))
                    entry.State = EntityState.Detached;
            }
        }
        var vencidas = await db.Ofertas.Where(o => o.Estado == EstadoOferta.Publicada && o.VigenciaHasta != null && o.VigenciaHasta < DateTime.Today).ToListAsync(cancellationToken);
        foreach (var oferta in vencidas) oferta.Estado = EstadoOferta.Vencida;
        if (vencidas.Count > 0) await db.SaveChangesAsync(cancellationToken);
        return nuevas;
    }
}