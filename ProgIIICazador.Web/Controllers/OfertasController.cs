using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Models;

namespace ProgIIICazador.Web.Controllers;

public class OfertasController(OfertasDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? marca, string? buscar)
    {
        var today = DateTime.UtcNow.Date;
        var query = db.Ofertas.AsNoTracking().Include(o => o.Empresa).Include(o => o.Beneficios)
            .Where(o => o.Estado == EstadoOferta.Publicada && (o.VigenciaDesde == null || o.VigenciaDesde <= today)
                && (o.VigenciaHasta == null || o.VigenciaHasta >= today));
        if (!string.IsNullOrWhiteSpace(marca)) query = query.Where(o => o.Empresa != null && o.Empresa.Slug == marca);
        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(o => o.Titulo.Contains(buscar) || o.Beneficio.Contains(buscar) || o.Condiciones.Contains(buscar)
                || o.Beneficios.Any(b => b.Descripcion.Contains(buscar)));
        ViewBag.Marcas = await db.Empresas.AsNoTracking().Where(e => e.Activa).OrderBy(e => e.Nombre).ToListAsync();
        ViewBag.MarcaSeleccionada = marca;
        ViewBag.Busqueda = buscar;
        return View(await query.OrderBy(o => o.VigenciaHasta).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var today = DateTime.UtcNow.Date;
        var oferta = await db.Ofertas.AsNoTracking().Include(o => o.Empresa).Include(o => o.Beneficios)
            .FirstOrDefaultAsync(o => o.Id == id && o.Estado == EstadoOferta.Publicada
                && (o.VigenciaDesde == null || o.VigenciaDesde <= today) && (o.VigenciaHasta == null || o.VigenciaHasta >= today));
        return oferta is null ? NotFound() : View(oferta);
    }
}