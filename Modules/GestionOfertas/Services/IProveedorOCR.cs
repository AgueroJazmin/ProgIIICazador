namespace ProgIIICazador.Web.Services.Procesamiento;

public interface IProveedorOCR
{
    Task<string> ExtraerTextoAsync(Stream archivo, string nombreArchivo, CancellationToken cancellationToken);
}