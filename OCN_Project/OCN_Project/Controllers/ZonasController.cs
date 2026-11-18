using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OCN_Project.Data;
using OCN_Project.Models.Entities;

namespace OCN_Project.Controllers;

/// <summary>CRUD de zonas de estudio con llaves foráneas hacia división y tipo de zona.</summary>
public class ZonasController : Controller
{
    private readonly OcnDbContext _context;

    public ZonasController(OcnDbContext context)
    {
        _context = context;
    }

    // GET: Zonas
    public async Task<IActionResult> Index()
    {
        var zonas = _context.Zonas
            .AsNoTracking()
            .Include(z => z.Division)
            .Include(z => z.TipoZona)
            .OrderBy(z => z.CodigoZona);

        return View(await zonas.ToListAsync());
    }

    // GET: Zonas/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var zona = await _context.Zonas
            .AsNoTracking()
            .Include(z => z.Division)
            .Include(z => z.TipoZona)
            .FirstOrDefaultAsync(z => z.IdZona == id);

        if (zona == null) return NotFound();

        return View(zona);
    }

    // GET: Zonas/Create
    public IActionResult Create()
    {
        PoblarListasDesplegables();
        return View();
    }

    // POST: Zonas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("IdZona,CodigoZona,Nombre,IdDivision,IdTipoZona,DescripcionGeneral,ReferenciaGeograficaAprox,Activo")] Zona zona)
    {
        if (ModelState.IsValid)
        {
            if (await CodigoDuplicado(zona.CodigoZona, zona.IdZona))
            {
                ModelState.AddModelError("CodigoZona", "Ya existe una zona con ese código.");
            }
            else
            {
                _context.Add(zona);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        PoblarListasDesplegables(zona.IdDivision, zona.IdTipoZona);
        return View(zona);
    }

    // GET: Zonas/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var zona = await _context.Zonas.FindAsync(id);
        if (zona == null) return NotFound();

        PoblarListasDesplegables(zona.IdDivision, zona.IdTipoZona);
        return View(zona);
    }

    // POST: Zonas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("IdZona,CodigoZona,Nombre,IdDivision,IdTipoZona,DescripcionGeneral,ReferenciaGeograficaAprox,Activo")] Zona zona)
    {
        if (id != zona.IdZona) return NotFound();

        if (ModelState.IsValid)
        {
            if (await CodigoDuplicado(zona.CodigoZona, zona.IdZona))
            {
                ModelState.AddModelError("CodigoZona", "Ya existe una zona con ese código.");
            }
            else
            {
                try
                {
                    _context.Update(zona);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ZonaExiste(zona.IdZona)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
        }
        PoblarListasDesplegables(zona.IdDivision, zona.IdTipoZona);
        return View(zona);
    }

    // GET: Zonas/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var zona = await _context.Zonas
            .AsNoTracking()
            .Include(z => z.Division)
            .Include(z => z.TipoZona)
            .FirstOrDefaultAsync(z => z.IdZona == id);

        if (zona == null) return NotFound();

        // Bloquea el borrado si otras tablas referencian la zona (FKs en Restrict).
        ViewBag.PuedeEliminar = !await TieneRegistrosAsociados(zona.IdZona);

        return View(zona);
    }

    // POST: Zonas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var zona = await _context.Zonas.FindAsync(id);
        if (zona != null)
        {
            if (await TieneRegistrosAsociados(id))
            {
                return await VolverAEliminarConBloqueo(id);
            }

            try
            {
                _context.Zonas.Remove(zona);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Red de seguridad: la base (database-first) puede tener FKs
                // adicionales no mapeadas en el modelo de EF.
                return await VolverAEliminarConBloqueo(id);
            }
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Recarga la zona con sus navegaciones y muestra el aviso de borrado bloqueado.</summary>
    private async Task<IActionResult> VolverAEliminarConBloqueo(int id)
    {
        var zona = await _context.Zonas
            .AsNoTracking()
            .Include(z => z.Division)
            .Include(z => z.TipoZona)
            .FirstOrDefaultAsync(z => z.IdZona == id);

        ViewBag.PuedeEliminar = false;
        return zona == null ? RedirectToAction(nameof(Index)) : View("Delete", zona);
    }

    /// <summary>Indica si otras tablas referencian la zona (el borrado sería rechazado por las FK).</summary>
    private async Task<bool> TieneRegistrosAsociados(int idZona)
    {
        return await _context.Levantamientos.AnyAsync(l => l.IdZona == idZona)
            || await _context.TendenciasProblematica.AnyAsync(t => t.IdZona == idZona)
            || await _context.RegistrosOcn.AnyAsync(r => r.IdZona == idZona)
            || await _context.RecursosZonasCobertura.AnyAsync(c => c.IdZona == idZona)
            || await _context.RecursosZonasBarrera.AnyAsync(b => b.IdZona == idZona)
            || await _context.RecursosZonasAccesibilidad.AnyAsync(a => a.IdZona == idZona)
            || await _context.PrioridadesCalculada.AnyAsync(p => p.IdZona == idZona)
            || await _context.BrechasAtencion.AnyAsync(b => b.IdZona == idZona);
    }

    private async Task<bool> ZonaExiste(int id)
    {
        return await _context.Zonas.AnyAsync(z => z.IdZona == id);
    }

    private async Task<bool> CodigoDuplicado(string codigoZona, int idZonaActual)
    {
        return await _context.Zonas
            .AnyAsync(z => z.CodigoZona == codigoZona && z.IdZona != idZonaActual);
    }

    private void PoblarListasDesplegables(int? idDivision = null, int? idTipoZona = null)
    {
        ViewBag.IdDivision = new SelectList(
            _context.DivisionesTerritoriales.AsNoTracking().Where(d => d.Activo).OrderBy(d => d.Nombre),
            "IdDivision", "Nombre", idDivision);

        ViewBag.IdTipoZona = new SelectList(
            _context.TiposZona.AsNoTracking().Where(t => t.Activo).OrderBy(t => t.Nombre),
            "IdTipoZona", "Nombre", idTipoZona);
    }
}
