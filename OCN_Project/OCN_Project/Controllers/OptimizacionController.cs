using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OCN_Project.Data;
using OCN_Project.Models;

namespace OCN_Project.Controllers;

/// <summary>
/// Controlador del módulo de optimización (tarea de tuning de consultas).
/// Las vistas GET presentan la explicación de cada caso; los POST ejecutan el
/// ciclo completo (consulta ANTES -> índice -> consulta DESPUÉS -> limpieza)
/// a través de OptimizacionExecutor y devuelven los resultados capturados.
/// </summary>
public class OptimizacionController : Controller
{
    private readonly IConfiguration _configuracion;
    private readonly IMemoryCache _cache;

    public OptimizacionController(IConfiguration configuracion, IMemoryCache cache)
    {
        _configuracion = configuracion;
        _cache = cache;
    }

    /// <summary>Cadena de conexión registrada en appsettings.json (autenticación de Windows).</summary>
    private string CadenaConexion => _configuracion.GetConnectionString("OcnConnection") ?? "";

    // GET: Optimizacion (portada de los 3 casos y su relación con los requisitos)
    public IActionResult Index()
    {
        return View();
    }

    // GET: Optimizacion/Caso1
    public IActionResult Caso1()
    {
        return View();
    }

    // GET: Optimizacion/Caso2
    public IActionResult Caso2()
    {
        return View();
    }

    // GET: Optimizacion/Caso3
    public IActionResult Caso3()
    {
        return View();
    }

    // POST: Optimizacion/EjecutarCaso1
    /// <summary>Ejecuta el ciclo completo del caso 1 (índice compuesto + histograma DBCC).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EjecutarCaso1()
    {
        var modelo = await OptimizacionExecutor.EjecutarCaso1Async(CadenaConexion);
        GuardarPlanesEnCache(modelo.Antes, modelo.Despues);
        return View("Caso1", modelo);
    }

    // POST: Optimizacion/EjecutarCaso2
    /// <summary>Ejecuta el ciclo completo del caso 2 (índice filtrado + veredicto automático).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EjecutarCaso2()
    {
        var modelo = await OptimizacionExecutor.EjecutarCaso2Async(CadenaConexion);
        GuardarPlanesEnCache(modelo.Antes, modelo.Despues);
        return View("Caso2", modelo);
    }

    // POST: Optimizacion/EjecutarCaso3
    /// <summary>Ejecuta el ciclo completo del caso 3 (columna calculada persistida + índice).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EjecutarCaso3()
    {
        var modelo = await OptimizacionExecutor.EjecutarCaso3Async(CadenaConexion);
        GuardarPlanesEnCache(modelo.Antes, modelo.Despues);
        return View("Caso3", modelo);
    }

    // GET: Optimizacion/DescargarPlan/{id}
    /// <summary>
    /// Descarga un plan de ejecución real (.sqlplan) desde la caché para abrirlo
    /// gráficamente en SSMS. Devuelve 404 si el plan no está (caducó a los ~10 min).
    /// </summary>
    public IActionResult DescargarPlan(string? id)
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var clave))
            return NotFound();

        if (!_cache.TryGetValue(clave, out string? xml) || string.IsNullOrEmpty(xml))
            return NotFound();

        var bytes = Encoding.UTF8.GetBytes(xml);
        return File(bytes, "application/octet-stream", $"plan_{id}.sqlplan");
    }

    /// <summary>Guarda en caché el XML de cada corrida por ~10 minutos para la descarga .sqlplan.</summary>
    private void GuardarPlanesEnCache(params ResultadoEjecucion?[] corridas)
    {
        foreach (var corrida in corridas)
        {
            if (corrida?.PlanXml == null)
                continue;
            _cache.Set(corrida.IdPlanCache, corrida.PlanXml, TimeSpan.FromMinutes(10));
        }
    }
}
