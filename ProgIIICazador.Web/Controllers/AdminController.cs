using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Models;

namespace ProgIIICazador.Web.Controllers;

public class AdminController(OfertasDbContext db, IConfiguration configuration) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
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
        var userOk = FixedEquals(model.Usuario, expectedUser ?? string.Empty);
        var passwordOk = FixedEquals(model.Contrasena, expectedPassword);
        if (!userOk || !passwordOk)
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
        return View(new RevisionCapturaViewModel { CapturaId = captura.Id, Titulo = captura.TituloDetectado ?? string.Empty,
            Descripcion = captura.TextoDetectado, UrlFuente = captura.Url });
    }

    [Authorize(Roles = "Administrador"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Revisar(RevisionCapturaViewModel model)
    {
        var captura = await db.CapturasFuente.Include(c => c.Empresa).FirstOrDefaultAsync(c => c.Id == model.CapturaId);
        if (captura is null) return NotFound();
        if (!Uri.TryCreate(model.UrlFuente, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            ModelState.AddModelError(nameof(model.UrlFuente), "La fuente debe ser una dirección HTTPS válida.");
        if (model.VigenciaDesde.HasValue && model.VigenciaHasta.HasValue && model.VigenciaHasta < model.VigenciaDesde)
            ModelState.AddModelError(nameof(model.VigenciaHasta), "La fecha de fin no puede ser anterior al inicio.");
        if (!ModelState.IsValid) return View(model);
        if (model.Publicar)
        {
            var oferta = new Oferta { EmpresaId = captura.EmpresaId, Titulo = model.Titulo.Trim(), Descripcion = model.Descripcion.Trim(),
                Beneficio = model.Beneficio.Trim(), Condiciones = model.Condiciones.Trim(), VigenciaDesde = model.VigenciaDesde,
                VigenciaHasta = model.VigenciaHasta, UrlFuente = model.UrlFuente, FechaCaptura = captura.FechaCapturaUtc,
                UltimaVerificacion = DateTime.UtcNow, Estado = EstadoOferta.Publicada, HuellaContenido = captura.HuellaContenido };
            db.Ofertas.Add(oferta);
            captura.Estado = EstadoCaptura.Procesada;
            captura.Observacion = "Aprobada y publicada por administración.";
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
    public async Task<IActionResult> EjecutarCaptura([FromServices] Services.Captura.ServicioCaptura servicio, CancellationToken cancellationToken)
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