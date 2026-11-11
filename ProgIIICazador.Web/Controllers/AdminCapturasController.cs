using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Models;
using ProgIIICazador.Web.Services.Procesamiento;

namespace ProgIIICazador.Web.Controllers;

[Authorize(Roles = "Administrador")]
[Route("AdminCapturas")]
public class AdminCapturasController(OfertasDbContext db, IProveedorOCR ocr) : Controller
{
    [HttpGet("Subir")]
    public async Task<IActionResult> Subir()
    {
        ViewBag.Empresas = await db.Empresas.Where(e => e.Activa).OrderBy(e => e.Nombre).ToListAsync();
        return View();
    }

    [HttpPost("Subir"), ValidateAntiForgeryToken, RequestSizeLimit(11_000_000)]
    public async Task<IActionResult> Subir(int empresaId, string urlFuente, IFormFile? archivo, CancellationToken cancellationToken)
    {
        ViewBag.Empresas = await db.Empresas.Where(e => e.Activa).OrderBy(e => e.Nombre).ToListAsync();
        if (archivo is null || archivo.Length == 0) ModelState.AddModelError("archivo", "Seleccioná un archivo.");
        if (archivo?.Length > 10_000_000) ModelState.AddModelError("archivo", "El archivo no puede superar los 10 MB.");
        if (!Uri.TryCreate(urlFuente, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            ModelState.AddModelError("urlFuente", "Ingresá la URL HTTPS de la publicación oficial.");
        if (!await db.Empresas.AnyAsync(e => e.Id == empresaId && e.Activa, cancellationToken))
            ModelState.AddModelError("empresaId", "Seleccioná una empresa válida.");
        if (!ModelState.IsValid) return View();
        try
        {
            await using var stream = archivo!.OpenReadStream();
            var text = await ocr.ExtraerTextoAsync(stream, Path.GetFileName(archivo.FileName), cancellationToken);
            if (string.IsNullOrWhiteSpace(text)) { ModelState.AddModelError("archivo", "No se detectó texto en el archivo."); return View(); }
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(urlFuente + text)));
            if (!await db.CapturasFuente.AnyAsync(c => c.HuellaContenido == hash, cancellationToken))
            {
                db.CapturasFuente.Add(new CapturaFuente { EmpresaId = empresaId, Url = urlFuente, TituloDetectado = Path.GetFileNameWithoutExtension(archivo.FileName),
                    TextoDetectado = text[..Math.Min(text.Length, 30000)], HuellaContenido = hash, Estado = EstadoCaptura.PendienteRevision,
                    Observacion = "Texto obtenido por OCR; requiere verificación contra la publicación original." });
                await db.SaveChangesAsync(cancellationToken);
            }
            TempData["OcrResultado"] = "Archivo procesado. El texto quedó pendiente de revisión administrativa.";
            return RedirectToAction("Index", "Admin");
        }
        catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TimeoutException)
        {
            ModelState.AddModelError("archivo", ex.Message);
            return View();
        }
    }
}