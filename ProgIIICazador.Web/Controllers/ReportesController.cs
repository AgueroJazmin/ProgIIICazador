using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Models;

namespace ProgIIICazador.Web.Controllers;

public class ReportesController(OfertasDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int? ofertaId)
    {
        await CargarOfertas();
        return View(new Reporte { OfertaId = ofertaId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reporte reporte)
    {
        var ofertaExiste = await db.Ofertas.AnyAsync(o => o.Id == reporte.OfertaId);
        if (!ofertaExiste)
            ModelState.AddModelError(nameof(reporte.OfertaId), "Seleccioná una oferta válida.");

        if (!ModelState.IsValid)
        {
            await CargarOfertas();
            return View(reporte);
        }

        reporte.FechaCreacion = DateTime.UtcNow;
        reporte.Estado = EstadoReporte.Pendiente;
        db.Reportes.Add(reporte);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Gracias));
    }

    public IActionResult Gracias() => View();

    private async Task CargarOfertas()
    {
        ViewBag.Ofertas = await db.Ofertas.AsNoTracking()
            .Include(o => o.Empresa)
            .OrderBy(o => o.Empresa!.Nombre)
            .ThenBy(o => o.Titulo)
            .ToListAsync();
    }
}