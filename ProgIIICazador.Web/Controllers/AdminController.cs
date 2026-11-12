using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Models;
using ProgIIICazador.Web.Services.Captura;
using ProgIIICazador.Web.Services.Procesamiento;

namespace ProgIIICazador.Web.Controllers;

public class AdminController(OfertasDbContext db, IConfiguration configuration, IClasificadorOfertas clasificador) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var expectedUser = configuration["Admin:Usuario"] ?? Environment.GetEnvironmentVariable("Admin__Usuario");
        var expectedPassword = configuration["Admin:Contrasena"] ?? Environment.GetEnvironmentVariable("Admin__Contrasena");
        if (string.IsNullOrWhiteSpace(expectedPassword))
        {
            ModelState.AddModelError(string.Empty, "El acceso administrativo no está configurado. Definí Admin__Contrasena como variable de entorno.");
            return View(model);
        }
        if (!FixedEquals(model.Usuario, expectedUser ?? string.Empty) || !FixedEquals(model.Contrasena, expectedPassword))
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            return View(model);
        }
        var claims = new[] { new Claim(ClaimTypes.Name, model.Usuario), new Claim(ClaimTypes.Role, "Administrador") };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false, AllowRefresh = true });
        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Index()
    {
        ViewBag.Pendientes = await db.CapturasFuente.CountAsync(c => c.Estado == EstadoCaptura.PendienteRevision);
        ViewBag.Reportes = await db.Reportes.CountAsync(r => r.Estado == EstadoReporte.Pendiente);
        ViewBag.Ofertas = await db.Ofertas.CountAsync();
        return View(await db.CapturasFuente.AsNoTracking().Include(c => c.Empresa)
            .Where(c => c.Estado == EstadoCaptura.PendienteRevision).OrderByDescending(c => c.FechaCapturaUtc).Take(100).ToListAsync());
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Revisar(int id)
    {
        var captura = await db.CapturasFuente.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (captura is null) return NotFound();
        ResultadoClasificacion result;
        try { result = await clasificador.ClasificarAsync(captura.TextoDetectado, HttpContext.RequestAborted); }
        catch (Exception) { result = new ResultadoClasificacion(false, null, null, null, null, null, null, null, null, 0, "No se pudo clasificar nuevamente; completar manualmente."); }
        ViewBag.Confianza = captura.ConfianzaClasificacion;
        ViewBag.EsOferta = captura.EsOfertaDetectada;
        return View(new RevisionCapturaViewModel {
            CapturaId = captura.Id, Titulo = result.Titulo ?? captura.TituloDetectado ?? string.Empty,
            // Oferta.Descripcion admite hasta 4.000 caracteres; el texto completo permanece en CapturaFuente.
            Descripcion = (captura.TextoDetectado ?? string.Empty)[..Math.Min(captura.TextoDetectado?.Length ?? 0, 4000)],
            Beneficio = result.Beneficio ?? string.Empty,
            Condiciones = result.Condiciones ?? string.Empty, TipoBeneficio = result.TipoBeneficio ?? "Beneficio",
            VigenciaDesde = result.Desde, VigenciaHasta = result.Hasta, Porcentaje = result.Porcentaje, Importe = result.Importe, UrlFuente = captura.Url
        });
    }

    [Authorize(Roles = "Administrador"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Revisar(RevisionCapturaViewModel model)
    {
        var captura = await db.CapturasFuente.FirstOrDefaultAsync(c => c.Id == model.CapturaId);
        if (captura is null) return NotFound();
        if (!Uri.TryCreate(model.UrlFuente, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo))
            ModelState.AddModelError(nameof(model.UrlFuente), "La fuente debe ser una dirección HTTPS válida.");
        else
        {
            var empresa = await db.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.Id == captura.EmpresaId);
            if (empresa is null || !Uri.TryCreate(empresa.UrlOficial, UriKind.Absolute, out var oficial) ||
                !(uri.Host.Equals(oficial.Host, StringComparison.OrdinalIgnoreCase) ||
                  uri.Host.EndsWith("." + oficial.Host, StringComparison.OrdinalIgnoreCase)))
                ModelState.AddModelError(nameof(model.UrlFuente), "La fuente debe pertenecer al dominio oficial de la empresa de la captura.");
        }
        if (captura.Estado != EstadoCaptura.PendienteRevision)
            return Conflict("Esta captura ya fue procesada o descartada.");
        if (model.VigenciaDesde.HasValue && model.VigenciaHasta.HasValue && model.VigenciaHasta.Value < model.VigenciaDesde.Value)
            ModelState.AddModelError(nameof(model.VigenciaHasta), "La fecha de fin no puede ser anterior al inicio.");
        if (model.Publicar && (string.IsNullOrWhiteSpace(model.Titulo) || string.IsNullOrWhiteSpace(model.Beneficio) || string.IsNullOrWhiteSpace(model.Condiciones)))
            ModelState.AddModelError(string.Empty, "Para publicar, completá y verificá el título, el beneficio y sus condiciones.");
        if (!ModelState.IsValid) return View(model);
        if (model.Publicar)
        {
            var oferta = await db.Ofertas.Include(o => o.Beneficios)
                .FirstOrDefaultAsync(o => o.UrlFuente == model.UrlFuente && o.Titulo == model.Titulo.Trim());
            if (oferta is null)
            {
                oferta = new Oferta { EmpresaId = captura.EmpresaId, FechaCaptura = captura.FechaCapturaUtc };
                db.Ofertas.Add(oferta);
                captura.Oferta = oferta;
            }
            else
            {
                captura.OfertaId = oferta.Id;
                oferta.Beneficios.Clear();
            }
            oferta.Titulo = model.Titulo.Trim();
            oferta.Descripcion = (model.Descripcion ?? string.Empty).Trim();
            oferta.Beneficio = model.Beneficio.Trim();
            oferta.Condiciones = model.Condiciones.Trim();
            oferta.VigenciaDesde = model.VigenciaDesde;
            oferta.VigenciaHasta = model.VigenciaHasta;
            oferta.UrlFuente = model.UrlFuente;
            oferta.UltimaVerificacion = DateTime.UtcNow;
            oferta.Estado = model.VigenciaHasta.HasValue && model.VigenciaHasta.Value < DateTime.Today
                ? EstadoOferta.Vencida : EstadoOferta.Publicada;
            oferta.HuellaContenido = captura.HuellaContenido;
            oferta.Beneficios.Add(new Beneficio {
                Tipo = string.IsNullOrWhiteSpace(model.TipoBeneficio) ? "Beneficio" : model.TipoBeneficio.Trim(),
                Descripcion = model.Beneficio.Trim(), Porcentaje = model.Porcentaje, Importe = model.Importe,
                TopeReintegro = model.TopeReintegro, Moneda = model.Importe.HasValue || model.TopeReintegro.HasValue ? "ARS" : null
            });
            foreach (var line in (model.BeneficiosAdicionales ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var parts = line.Split('|', 2, StringSplitOptions.TrimEntries);
                if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                    oferta.Beneficios.Add(new Beneficio { Tipo = parts[0][..Math.Min(parts[0].Length, 80)], Descripcion = parts[1][..Math.Min(parts[1].Length, 500)] });
            }
            captura.Estado = EstadoCaptura.Procesada;
            captura.Observacion = "Aprobada por administración después de contrastar con la fuente original.";
        }
        else
        {
            captura.Estado = EstadoCaptura.Descartada;
            captura.Observacion = "Descartada por administración.";
        }
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolverReporte(int id, EstadoReporte estado)
    {
        var reporte = await db.Reportes.FindAsync(id);
        if (reporte is null) return NotFound();
        if (estado is not (EstadoReporte.Resuelto or EstadoReporte.Descartado)) return BadRequest();
        reporte.Estado = estado;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Reportes));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Reportes() => View(await db.Reportes.AsNoTracking().Include(r => r.Oferta)
        .ThenInclude(o => o!.Empresa).OrderBy(r => r.Estado).ThenByDescending(r => r.FechaCreacion).ToListAsync());

    [Authorize(Roles = "Administrador"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EjecutarCaptura([FromServices] ServicioCaptura servicio, CancellationToken cancellationToken)
    {
        TempData["CapturaResultado"] = $"Se detectaron {await servicio.EjecutarAsync(cancellationToken)} candidatos nuevos para revisar.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrador"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private static bool FixedEquals(string actual, string expected)
    {
        var a = SHA256.HashData(Encoding.UTF8.GetBytes(actual));
        var b = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}