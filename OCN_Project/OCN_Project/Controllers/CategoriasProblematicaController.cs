using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCN_Project.Data;
using OCN_Project.Models.Entities;

namespace OCN_Project.Controllers;

/// <summary>CRUD simple del catálogo de categorías de problemática.</summary>
public class CategoriasProblematicaController : Controller
{
    private readonly OcnDbContext _context;

    public CategoriasProblematicaController(OcnDbContext context)
    {
        _context = context;
    }

    // GET: CategoriasProblematica
    public async Task<IActionResult> Index()
    {
        return View(await _context.CategoriasProblematica
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync());
    }

    // GET: CategoriasProblematica/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var categoria = await _context.CategoriasProblematica
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCategoria == id);

        if (categoria == null) return NotFound();

        return View(categoria);
    }

    // GET: CategoriasProblematica/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: CategoriasProblematica/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("IdCategoria,Nombre,Activo")] CategoriaProblematica categoria)
    {
        if (ModelState.IsValid)
        {
            if (await NombreDuplicado(categoria.Nombre, categoria.IdCategoria))
            {
                ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre.");
            }
            else
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        return View(categoria);
    }

    // GET: CategoriasProblematica/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var categoria = await _context.CategoriasProblematica.FindAsync(id);
        if (categoria == null) return NotFound();

        return View(categoria);
    }

    // POST: CategoriasProblematica/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("IdCategoria,Nombre,Activo")] CategoriaProblematica categoria)
    {
        if (id != categoria.IdCategoria) return NotFound();

        if (ModelState.IsValid)
        {
            if (await NombreDuplicado(categoria.Nombre, categoria.IdCategoria))
            {
                ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre.");
            }
            else
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CategoriaExiste(categoria.IdCategoria)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
        }
        return View(categoria);
    }

    // GET: CategoriasProblematica/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var categoria = await _context.CategoriasProblematica
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCategoria == id);

        if (categoria == null) return NotFound();

        return View(categoria);
    }

    // POST: CategoriasProblematica/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categoria = await _context.CategoriasProblematica.FindAsync(id);
        if (categoria != null)
        {
            _context.CategoriasProblematica.Remove(categoria);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CategoriaExiste(int id)
    {
        return await _context.CategoriasProblematica.AnyAsync(c => c.IdCategoria == id);
    }

    private async Task<bool> NombreDuplicado(string nombre, int idActual)
    {
        return await _context.CategoriasProblematica
            .AnyAsync(c => c.Nombre == nombre && c.IdCategoria != idActual);
    }
}
