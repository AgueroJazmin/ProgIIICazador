using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgIIICazador.Web.Data;
using ProgIIICazador.Web.Models;

namespace ProgIIICazador.Web.Controllers;

public class OfertasController(OfertasDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? marca, string? buscar)
    {
        var query = db.Ofertas
            .AsNoTracking()
            .Include(o => o.Empresa)
            .Where(o => o.Estado == EstadoOferta.Publicada &&
                (o.VigenciaHasta == null || o.VigenciaHasta >= DateTime.Today));

        if (!string.IsNullOrWhiteSpace(marca))
            query = query.Where(o => o.Empresa != null && o.Empresa.Slug == marca);

        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(o => o.Titulo.Contains(buscar) || o.Beneficio.Contains(buscar) || o.Condiciones.Contains(buscar));

        ViewBag.Marcas = await db.Empresas.AsNoTracking().Where(e => e.Activa).OrderBy(e => e.Nombre).ToListAsync();
        ViewBag.MarcaSeleccionada = marca;
        ViewBag.Busqueda = buscar;
        return View(await query.OrderBy(o => o.VigenciaHasta).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var oferta = await db.Ofertas.AsNoTracking().Include(o => o.Empresa)
            .FirstOrDefaultAsync(o => o.Id == id && o.Estado == EstadoOferta.Publicada);
        return oferta is null ? NotFound() : View(oferta);
    }
}