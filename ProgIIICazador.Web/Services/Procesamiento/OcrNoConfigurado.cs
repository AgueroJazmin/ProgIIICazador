namespace ProgIIICazador.Web.Services.Procesamiento;

/// <summary>Proveedor seguro por defecto: no simula OCR ni devuelve texto inventado.</summary>
public sealed class OcrNoConfigurado : IProveedorOCR
{
    public Task<string> ExtraerTextoAsync(Stream archivo, string nombreArchivo, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("OCR no configurado. Definí un proveedor compatible en la configuración antes de procesar archivos.");
}