namespace ProgIIICazador.Web.Services.Captura;

public interface IExtractorFuente
{
    Task<IReadOnlyCollection<ResultadoExtraccion>> ExtraerAsync(Uri url, CancellationToken cancellationToken);
}