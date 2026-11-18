using Microsoft.AspNetCore.Mvc;
using OCN_Project.Data;

namespace OCN_Project.Controllers;

/// <summary>
/// Controlador de demostraciones en vivo de programación T-SQL (tarea universitaria).
/// Cada acción muestra la teoría y, al ejecutar la demostración (POST), la evidencia
/// antes/después contra SQL Server. La CTE recursiva es de solo lectura y corre en GET.
/// </summary>
public class ProgramacionTSqlController : Controller
{
    private readonly IConfiguration _configuration;

    /// <summary>Inyecta la configuración para leer la cadena de conexión OcnConnection.</summary>
    public ProgramacionTSqlController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Cadena de conexión del proyecto (Windows auth contra localhost).</summary>
    private string CadenaConexion =>
        _configuration.GetConnectionString("OcnConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'OcnConnection' en la configuración.");

    /// <summary>Página de la demostración del procedimiento almacenado transaccional.</summary>
    public IActionResult ProcedimientoTransaccional()
    {
        ViewData["Title"] = "Procedimiento transaccional";
        return View();
    }

    /// <summary>Ejecuta la demostración del procedimiento transaccional (éxito + rollback).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ProcedimientoTransaccionalPost()
    {
        ViewData["Title"] = "Procedimiento transaccional";
        return View("ProcedimientoTransaccional", TSqlDemoExecutor.ProcedimientoTransaccional(CadenaConexion));
    }

    /// <summary>Página de la demostración del trigger de auditoría de estado.</summary>
    public IActionResult TriggerAuditoria()
    {
        ViewData["Title"] = "Trigger de auditoría";
        return View();
    }

    /// <summary>Ejecuta la demostración del trigger de auditoría (cambio de estado vs. edición sin auditoría).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TriggerAuditoriaPost()
    {
        ViewData["Title"] = "Trigger de auditoría";
        return View("TriggerAuditoria", TSqlDemoExecutor.TriggerAuditoria(CadenaConexion));
    }

    /// <summary>Página de la demostración del trigger de protección de borrado.</summary>
    public IActionResult TriggerEliminacion()
    {
        ViewData["Title"] = "Trigger de eliminación";
        return View();
    }

    /// <summary>Ejecuta la demostración del trigger de eliminación (bloquea 'Completada', permite 'Planeada').</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TriggerEliminacionPost()
    {
        ViewData["Title"] = "Trigger de eliminación";
        return View("TriggerEliminacion", TSqlDemoExecutor.TriggerEliminacion(CadenaConexion));
    }

    /// <summary>Página de la demostración del cursor de procesamiento de intervenciones antiguas.</summary>
    public IActionResult Cursor()
    {
        ViewData["Title"] = "Cursor";
        return View();
    }

    /// <summary>Ejecuta la demostración del cursor (procesa intervenciones antiguas fila a fila).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CursorPost()
    {
        ViewData["Title"] = "Cursor";
        return View("Cursor", TSqlDemoExecutor.Cursor(CadenaConexion));
    }

    /// <summary>
    /// Página de la CTE recursiva de problemáticas. Es de solo lectura, por lo que
    /// ejecuta la consulta directamente en la acción GET sin requerir POST.
    /// </summary>
    public IActionResult CteRecursiva()
    {
        ViewData["Title"] = "CTE recursiva";
        return View(TSqlDemoExecutor.CteRecursiva(CadenaConexion));
    }
}
