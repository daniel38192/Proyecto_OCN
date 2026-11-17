using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OCN_Project.Data;
using OCN_Project.Models.Entities;

namespace OCN_Project.Controllers;

/// <summary>CRUD de periodos de estudio con validación de fechas.</summary>
public class PeriodosController : Controller
{
    private readonly OcnDbContext _context;

    public PeriodosController(OcnDbContext context)
    {
        _context = context;
    }

    // GET: Periodos
    public async Task<IActionResult> Index()
    {
        return View(await _context.Periodos
            .AsNoTracking()
            .OrderByDescending(p => p.FechaInicio)
            .ToListAsync());
    }

    // GET: Periodos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var periodo = await _context.Periodos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPeriodo == id);

        if (periodo == null) return NotFound();

        return View(periodo);
    }

    // GET: Periodos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Periodos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("IdPeriodo,CodigoPeriodo,Nombre,FechaInicio,FechaFin,Activo")] Periodo periodo)
    {
        if (ModelState.IsValid)
        {
            if (periodo.FechaFin.HasValue && periodo.FechaFin < periodo.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin no puede ser anterior a la fecha de inicio.");
            }
            else if (await CodigoDuplicado(periodo.CodigoPeriodo, periodo.IdPeriodo))
            {
                ModelState.AddModelError("CodigoPeriodo", "Ya existe un periodo con ese código.");
            }
            else
            {
                _context.Add(periodo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }
        return View(periodo);
    }

    // GET: Periodos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var periodo = await _context.Periodos.FindAsync(id);
        if (periodo == null) return NotFound();

        return View(periodo);
    }

    // POST: Periodos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("IdPeriodo,CodigoPeriodo,Nombre,FechaInicio,FechaFin,Activo")] Periodo periodo)
    {
        if (id != periodo.IdPeriodo) return NotFound();

        if (ModelState.IsValid)
        {
            if (periodo.FechaFin.HasValue && periodo.FechaFin < periodo.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin no puede ser anterior a la fecha de inicio.");
            }
            else if (await CodigoDuplicado(periodo.CodigoPeriodo, periodo.IdPeriodo))
            {
                ModelState.AddModelError("CodigoPeriodo", "Ya existe un periodo con ese código.");
            }
            else
            {
                try
                {
                    _context.Update(periodo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PeriodoExiste(periodo.IdPeriodo)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
        }
        return View(periodo);
    }

    // GET: Periodos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var periodo = await _context.Periodos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPeriodo == id);

        if (periodo == null) return NotFound();

        return View(periodo);
    }

    // POST: Periodos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var periodo = await _context.Periodos.FindAsync(id);
        if (periodo != null)
        {
            _context.Periodos.Remove(periodo);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> PeriodoExiste(int id)
    {
        return await _context.Periodos.AnyAsync(p => p.IdPeriodo == id);
    }

    private async Task<bool> CodigoDuplicado(string codigo, int idActual)
    {
        return await _context.Periodos
            .AnyAsync(p => p.CodigoPeriodo == codigo && p.IdPeriodo != idActual);
    }
}
