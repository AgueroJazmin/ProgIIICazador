namespace ProgIIICazador.Web.Modules.GestionOfertas.Services;

public interface IProveedorOCR
{
    Task<string> ExtraerTextoAsync(Stream archivo, string nombreArchivo, CancellationToken cancellationToken);
}