using System.Text;

namespace OCN_Project.Models;

/// <summary>Modelos de vista compartidos por las páginas de demostración de programación T-SQL.
/// Cada demostración construye un <see cref="DemoResultado"/> con la evidencia
/// (tablas antes/después), los pasos ejecutados y la justificación de diseño.
/// La clase <see cref="TablaDatos"/> ya está definida en OptimizacionViewModels.cs y se reutiliza.</summary>

/// <summary>Bloque de evidencia con título, tabla opcional y notas adicionales.</summary>
public class SeccionEvidencia
{
    public string Titulo { get; set; } = string.Empty;

    public TablaDatos? Tabla { get; set; }

    public List<string> Notas { get; set; } = new();
}

/// <summary>Resultado completo de una demostración T-SQL (éxito, pasos, evidencias y justificación).</summary>
public class DemoResultado
{
    public bool Exito { get; set; }

    /// <summary>Mensaje de error en español cuando <see cref="Exito"/> es false.</summary>
    public string? Error { get; set; }

    /// <summary>Bitácora ordenada de los pasos ejecutados en la demostración.</summary>
    public List<string> Pasos { get; set; } = new();

    public List<SeccionEvidencia> Secciones { get; set; } = new();

    /// <summary>Texto con la decisión de diseño concreta que se defiende en la tarea.</summary>
    public string Justificacion { get; set; } = string.Empty;
}
