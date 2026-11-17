using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OCN_Project.Data;
using OCN_Project.Models.Entities;

namespace OCN_Project.Controllers;

/// <summary>CRUD de problemáticas con categoría y subproblemática (jerárquica).</summary>
public class ProblematicasController : Controller
{
    private readonly OcnDbContext _context;

    public ProblematicasController(OcnDbContext context)
    {
        _context = context;
    }

    // GET: Problematicas
    public async Task<IActionResult> Index()
    {
        var problematicas = _context.Problematicas
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.ProblematicaPadre)
            .OrderBy(p => p.Categoria.Nombre).ThenBy(p => p.Nombre);

        return View(await problematicas.ToListAsync());
    }

    // GET: Problematicas/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var problematica = await _context.Problematicas
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.ProblematicaPadre)
            .FirstOrDefaultAsync(p => p.IdProblematica == id);

        if (problematica == null) return NotFound();

        return View(problematica);
    }

    // GET: Problematicas/Create
    public IActionResult Create()
    {
        PoblarListasDesplegables();
        return View();
    }

    // POST: Problematicas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("IdProblematica,IdCategoria,IdProblematicaPadre,Nombre,Descripcion,Activo")] Problematica problematica)
    {
        if (ModelState.IsValid)
        {
            if (problematica.IdProblematicaPadre == problematica.IdProblematica)
            {
                ModelState.AddModelError("IdProblematicaPadre", "Una problemática no puede ser su propia subproblemática.");
            }
            else
            {
                _context.Add(problematica);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        PoblarListasDesplegables(problematica.IdCategoria, problematica.IdProblematicaPadre);
        return View(problematica);
    }

    // GET: Problematicas/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var problematica = await _context.Problematicas.FindAsync(id);
        if (problematica == null) return NotFound();

        PoblarListasDesplegables(problematica.IdCategoria, problematica.IdProblematicaPadre);
        return View(problematica);
    }

    // POST: Problematicas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("IdProblematica,IdCategoria,IdProblematicaPadre,Nombre,Descripcion,Activo")] Problematica problematica)
    {
        if (id != problematica.IdProblematica) return NotFound();

        if (ModelState.IsValid)
        {
            if (problematica.IdProblematicaPadre == problematica.IdProblematica)
            {
                ModelState.AddModelError("IdProblematicaPadre", "Una problemática no puede ser su propia subproblemática.");
            }
            else
            {
                try
                {
                    _context.Update(problematica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProblematicaExiste(problematica.IdProblematica)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
        }
        PoblarListasDesplegables(problematica.IdCategoria, problematica.IdProblematicaPadre);
        return View(problematica);
    }

    // GET: Problematicas/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var problematica = await _context.Problematicas
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.ProblematicaPadre)
            .FirstOrDefaultAsync(p => p.IdProblematica == id);

        if (problematica == null) return NotFound();

        return View(problematica);
    }

    // POST: Problematicas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var problematica = await _context.Problematicas.FindAsync(id);
        if (problematica != null)
        {
            _context.Problematicas.Remove(problematica);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ProblematicaExiste(int id)
    {
        return await _context.Problematicas.AnyAsync(p => p.IdProblematica == id);
    }

    private void PoblarListasDesplegables(int? idCategoria = null, int? idProblematicaPadre = null)
    {
        ViewBag.IdCategoria = new SelectList(
            _context.CategoriasProblematica.AsNoTracking().Where(c => c.Activo).OrderBy(c => c.Nombre),
            "IdCategoria", "Nombre", idCategoria);

        ViewBag.IdProblematicaPadre = new SelectList(
            _context.Problematicas.AsNoTracking().Where(p => p.Activo).OrderBy(p => p.Nombre),
            "IdProblematica", "Nombre", idProblematicaPadre);
    }
}
