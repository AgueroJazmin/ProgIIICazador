namespace ProgIIICazador.Web.Modules.CapturaOfertas.Extractors;

public interface IExtractorFuente
{
    Task<IReadOnlyCollection<ResultadoExtraccion>> ExtraerAsync(Uri url, CancellationToken cancellationToken);
}