using System.Text.RegularExpressions;

namespace OCN_Project.Models;

/// <summary>
/// Modelos de vista para el módulo de optimización (tarea de tuning).
/// Todo se serializa como texto para que las vistas Razor solo dibujen
/// columnas y filas sin conocer los tipos reales de SQL Server.
/// </summary>

/// <summary>Tabla genérica de resultados SQL (columnas + filas ya formateadas como texto).</summary>
public class TablaDatos
{
    /// <summary>Nombres de las columnas devueltas por la consulta.</summary>
    public List<string> Columnas { get; set; } = new();

    /// <summary>Filas con las celdas ya convertidas a texto de presentación.</summary>
    public List<string[]> Filas { get; set; } = new();
}

/// <summary>Un operador físico (RelOp) del plan de ejecución real.</summary>
public class OperadorPlan
{
    /// <summary>Operador físico (PhysicalOp): Index Seek, Clustered Index Scan, etc.</summary>
    public string OperadorFisico { get; set; } = "";

    /// <summary>Operador lógico (LogicalOp): IndexRangeScan, ClusteredIndexScan, etc.</summary>
    public string OperadorLogico { get; set; } = "";

    /// <summary>Tabla e índice referenciados por el operador (si los hay).</summary>
    public string Objeto { get; set; } = "";

    /// <summary>Costo estimado del subárbol (EstimatedTotalSubtreeCost).</summary>
    public double CostoSubarbol { get; set; }

    /// <summary>Filas estimadas por el optimizador (EstimateRows).</summary>
    public double FilasEstimadas { get; set; }

    /// <summary>Filas realmente procesadas (ActualRows, disponible con STATISTICS XML).</summary>
    public double FilasReales { get; set; }

    /// <summary>Indica si el XML aportó el atributo ActualRows para este operador.</summary>
    public bool FilasRealesDisponibles { get; set; }
}

/// <summary>Resumen legible de un plan de ejecución real (showplan XML).</summary>
public class PlanResumen
{
    /// <summary>Costo total estimado del statement (StmtSimple/@StatementSubTreeCost).</summary>
    public double CostoEstimado { get; set; }

    /// <summary>Filas estimadas por el optimizador para el statement (StmtSimple/@StatementEstRows).</summary>
    public double FilasEstimadas { get; set; }

    /// <summary>Nivel de optimización alcanzado (StmtSimple/@StatementOptmLevel): TRIVIAL, FULL, etc.</summary>
    public string NivelOptimizacion { get; set; } = "";

    /// <summary>Grado de paralelismo del plan (QueryPlan/@DegreeOfParallelism).</summary>
    public int GradoParalelismo { get; set; }

    /// <summary>Operadores del plan en orden de aparición.</summary>
    public List<OperadorPlan> Operadores { get; set; } = new();

    /// <summary>Advertencias del plan (Warnings): conversiones, spills, índices faltantes, etc.</summary>
    public List<string> Advertencias { get; set; } = new();
}

/// <summary>Resultado de una corrida (ANTES o DESPUÉS) con captura de STATISTICS IO/TIME y plan real.</summary>
public class ResultadoEjecucion
{
    /// <summary>Título del bloque: "ANTES (sin índice)" o "DESPUÉS (con índice)".</summary>
    public string Titulo { get; set; } = "";

    /// <summary>Rejilla de datos devuelta por la consulta.</summary>
    public TablaDatos TablaDatos { get; set; } = new();

    /// <summary>Mensajes de STATISTICS IO capturados (lecturas lógicas/físicas por tabla).</summary>
    public List<string> MensajesIo { get; set; } = new();

    /// <summary>Mensajes de STATISTICS TIME capturados (CPU y tiempo transcurrido).</summary>
    public List<string> MensajesTiempo { get; set; } = new();

    /// <summary>Otros mensajes informativos que no son IO ni TIME (por transparencia).</summary>
    public List<string> MensajesOtros { get; set; } = new();

    /// <summary>Plan de ejecución real en XML (para descargarlo como .sqlplan).</summary>
    public string? PlanXml { get; set; }

    /// <summary>Resumen parseado del plan (null si el XML no llegó o no se pudo interpretar).</summary>
    public PlanResumen? Plan { get; set; }

    /// <summary>Duración total de la corrida en milisegundos (reloj de la app).</summary>
    public long DuracionMs { get; set; }

    /// <summary>Clave con la que el plan XML queda en caché para descargarse como .sqlplan.</summary>
    public Guid IdPlanCache { get; set; }

    /// <summary>Total de lecturas lógicas sumado a partir de los mensajes de STATISTICS IO.</summary>
    public long TotalLecturasLogicas => SumarLecturasLogicas(MensajesIo);

    /// <summary>
    /// Suma las lecturas lógicas de los mensajes de STATISTICS IO (se excluye "lob logical reads"
    /// con un lookbehind para no duplicar; se acepta también el texto en español).
    /// </summary>
    private static long SumarLecturasLogicas(List<string> mensajesIo)
    {
        long total = 0;
        foreach (var mensaje in mensajesIo)
        {
            foreach (Match coincidencia in Regex.Matches(mensaje, @"(?<!lob\s)(?:logical reads|lecturas lógicas)\s+(\d+)"))
                total += long.TryParse(coincidencia.Groups[1].Value, out var valor) ? valor : 0;
        }
        return total;
    }
}

/// <summary>Resultado completo del caso 1 (tratamiento completo: índice compuesto + histograma DBCC).</summary>
public class Caso1Resultado
{
    /// <summary>Corrida ANTES de crear el índice.</summary>
    public ResultadoEjecucion? Antes { get; set; }

    /// <summary>Corrida DESPUÉS de crear el índice.</summary>
    public ResultadoEjecucion? Despues { get; set; }

    /// <summary>Primer conjunto de DBCC SHOW_STATISTICS (STAT_HEADER).</summary>
    public TablaDatos Encabezado { get; set; } = new();

    /// <summary>Segundo conjunto de DBCC SHOW_STATISTICS (DENSITY_VECTOR).</summary>
    public TablaDatos Densidades { get; set; } = new();

    /// <summary>Tercer conjunto de DBCC SHOW_STATISTICS (HISTOGRAM).</summary>
    public TablaDatos Histograma { get; set; } = new();

    /// <summary>Lectura automática del histograma orientada al predicado idzona = 1.</summary>
    public string LecturaHistograma { get; set; } = "";

    /// <summary>Nombre del índice creado (y eliminado al final de la corrida).</summary>
    public string NombreIndice { get; set; } = "";

    /// <summary>Definición T-SQL del índice (como en el guion de la tarea).</summary>
    public string DefinicionIndice { get; set; } = "";

    /// <summary>Bitácora de pasos ejecutados (una línea por paso con OK/ERROR).</summary>
    public List<string> Pasos { get; set; } = new();

    /// <summary>Mensaje de error si la corrida falló (la limpieza se ejecuta igual).</summary>
    public string? Error { get; set; }

    /// <summary>Indica si el ciclo completo terminó sin errores.</summary>
    public bool Exito { get; set; }
}

/// <summary>Resultado de los casos 2 y 3 (ciclo simple antes/después con veredicto automático).</summary>
public class CasoComparacionResultado
{
    /// <summary>Corrida ANTES de crear el índice.</summary>
    public ResultadoEjecucion? Antes { get; set; }

    /// <summary>Corrida DESPUÉS de crear el índice.</summary>
    public ResultadoEjecucion? Despues { get; set; }

    /// <summary>Nombre del índice creado (y eliminado al final de la corrida).</summary>
    public string NombreIndice { get; set; } = "";

    /// <summary>Definición T-SQL del índice (como en el guion de la tarea).</summary>
    public string DefinicionIndice { get; set; } = "";

    /// <summary>Bitácora de pasos ejecutados (una línea por paso con OK/ERROR).</summary>
    public List<string> Pasos { get; set; } = new();

    /// <summary>Indica si el plan DESPUÉS realmente usó el índice (veredicto automático).</summary>
    public bool IndiceUsadoEnDespues { get; set; }

    /// <summary>Nota de aprendizaje cuando el optimizador no usó el índice (requisito 3 de la tarea).</summary>
    public string? NotaAprendizaje { get; set; }

    /// <summary>
    /// Solo caso 3: verdadero cuando la app agregó la columna calculada 'duracion'
    /// (porque no existía) y la eliminó al terminar la corrida.
    /// </summary>
    public bool ColumnaCalculadaAgregadaYEliminada { get; set; }

    /// <summary>Mensaje de error si la corrida falló (la limpieza se ejecuta igual).</summary>
    public string? Error { get; set; }

    /// <summary>Indica si el ciclo completo terminó sin errores.</summary>
    public bool Exito { get; set; }
}
