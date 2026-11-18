using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OCN_Project.Data;
using OCN_Project.Models.Entities;

namespace OCN_Project.Controllers;

/// <summary>
/// CRUD del módulo principal RegistrosOCN: FKs a Zona y Problematica,
/// validación de rango 1-5 en Intensidad/Frecuencia y AntiForgeryToken.
/// </summary>
public class RegistrosOcnController : Controller
{
    private readonly OcnDbContext _context;

    public RegistrosOcnController(OcnDbContext context)
    {
        _context = context;
    }

    // GET: RegistrosOcn
    public async Task<IActionResult> Index()
    {
        var registros = _context.RegistrosOcn
            .AsNoTracking()
            .Include(r => r.Zona)
            .Include(r => r.Problematica)
            .OrderByDescending(r => r.FechaRegistro);

        return View(await registros.ToListAsync());
    }

    // GET: RegistrosOcn/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var registro = await _context.RegistrosOcn
            .AsNoTracking()
            .Include(r => r.Zona)
            .Include(r => r.Problematica)
            .FirstOrDefaultAsync(r => r.IdRegistroOcn == id);

        if (registro == null) return NotFound();

        return View(registro);
    }

    // GET: RegistrosOcn/Create
    public IActionResult Create()
    {
        PoblarListasDesplegables();
        return View();
    }

    // POST: RegistrosOcn/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("IdZona,IdProblematica,Intensidad,Frecuencia,NotaContexto,FechaRegistro")] RegistroOcn registro)
    {
        if (ModelState.IsValid)
        {
            registro.FechaRegistro = DateTime.Now;
            _context.Add(registro);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        PoblarListasDesplegables(registro.IdZona, registro.IdProblematica);
        return View(registro);
    }

    // GET: RegistrosOcn/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var registro = await _context.RegistrosOcn.FindAsync(id);
        if (registro == null) return NotFound();

        PoblarListasDesplegables(registro.IdZona, registro.IdProblematica);
        return View(registro);
    }

    // POST: RegistrosOcn/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("IdRegistroOcn,IdZona,IdProblematica,Intensidad,Frecuencia,NotaContexto,FechaRegistro")] RegistroOcn registro)
    {
        if (id != registro.IdRegistroOcn) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(registro);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RegistroExiste(registro.IdRegistroOcn)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        PoblarListasDesplegables(registro.IdZona, registro.IdProblematica);
        return View(registro);
    }

    // GET: RegistrosOcn/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var registro = await _context.RegistrosOcn
            .AsNoTracking()
            .Include(r => r.Zona)
            .Include(r => r.Problematica)
            .FirstOrDefaultAsync(r => r.IdRegistroOcn == id);

        if (registro == null) return NotFound();

        // RegistroOcn no tiene tablas hijas en el modelo; el borrado siempre está habilitado.
        ViewBag.PuedeEliminar = true;

        return View(registro);
    }

    // POST: RegistrosOcn/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var registro = await _context.RegistrosOcn.FindAsync(id);
        if (registro != null)
        {
            try
            {
                _context.RegistrosOcn.Remove(registro);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Red de seguridad: la base (database-first) puede tener FKs
                // hacia registroocn no mapeadas en el modelo de EF.
                return await VolverAEliminarConBloqueo(id);
            }
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Recarga el registro con sus navegaciones y muestra el aviso de borrado bloqueado.</summary>
    private async Task<IActionResult> VolverAEliminarConBloqueo(int id)
    {
        var registro = await _context.RegistrosOcn
            .AsNoTracking()
            .Include(r => r.Zona)
            .Include(r => r.Problematica)
            .FirstOrDefaultAsync(r => r.IdRegistroOcn == id);

        ViewBag.PuedeEliminar = false;
        return registro == null ? RedirectToAction(nameof(Index)) : View("Delete", registro);
    }

    private async Task<bool> RegistroExiste(int id)
    {
        return await _context.RegistrosOcn.AnyAsync(r => r.IdRegistroOcn == id);
    }

    private void PoblarListasDesplegables(int? idZona = null, int? idProblematica = null)
    {
        ViewBag.IdZona = new SelectList(
            _context.Zonas.AsNoTracking().Where(z => z.Activo).OrderBy(z => z.CodigoZona),
            "IdZona", "Nombre", idZona);

        ViewBag.IdProblematica = new SelectList(
            _context.Problematicas.AsNoTracking().Where(p => p.Activo).OrderBy(p => p.Nombre),
            "IdProblematica", "Nombre", idProblematica);
    }
}
