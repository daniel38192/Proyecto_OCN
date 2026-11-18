using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using OCN_Project.Models;

namespace OCN_Project.Data;

/// <summary>
/// Ejecutor de los 3 casos de optimización del guion de la tarea (ADO.NET directo).
/// Para cada caso corre el ciclo completo sobre UNA sola conexión:
/// consulta ANTES (con STATISTICS IO/TIME/XML) -> DDL del índice -> consulta DESPUÉS
/// -> limpieza en finally para dejar la base exactamente como estaba.
/// </summary>
public static class OptimizacionExecutor
{
    // ============================================================
    // SQL del guion (constantes revisables, iguales a la tarea)
    // ============================================================

    // CASO 1: consulta sobre levantamientos de la zona 1 (índice compuesto).
    private const string SqlCaso1Consulta = @"
select [idlevantamiento], [codigolevantamiento], [idperiodo], [idmetodo], [fechalevantamiento], [numeroparticipantesaprox], [observacionesmetodologicas], [fechacaptura]
from ocn.levantamiento
where idzona = 1
order by fechalevantamiento desc";

    // CASO 1: índice compuesto. NOTA: el guion dice "using btree", sintaxis NO válida en
    // SQL Server para índices no espaciales; se omite esa cláusula.
    private const string SqlCaso1CrearIndice = @"
create index idx_levantamiento_idzona_fechalevantamiento
on ocn.levantamiento (idzona, fechalevantamiento)
include (codigolevantamiento, idperiodo, idmetodo, numeroparticipantesaprox)";

    // CASO 1: limpieza (DROP INDEX IF EXISTS requiere SQL Server 2016+).
    private const string SqlCaso1SoltarIndice = @"
drop index if exists ocn.levantamiento.idx_levantamiento_idzona_fechalevantamiento";

    // CASO 1: histograma de la estadística creada con el índice (3 conjuntos de resultados).
    private const string SqlCaso1Dbcc = @"
dbcc show_statistics('ocn.levantamiento', 'idx_levantamiento_idzona_fechalevantamiento')
with stat_header, density_vector, histogram";

    // CASO 2: intervenciones planeadas/en proceso registradas en enero de 2025 (índice filtrado).
    private const string SqlCaso2Consulta = @"
select [idintervencion], [idtipointervencion], [idobservacionproblematica], [idrecurso], [fechacompletado], [estado]
from ocn.intervencion i
where i.estado in ('Planeada', 'EnProceso')
and i.fecharegistro >= '20250101' and fecharegistro < '20250201'";

    // CASO 2: índice filtrado (solo filas Planeada/EnProceso).
    private const string SqlCaso2CrearIndice = @"
create nonclustered index idx_intervencion_estado_fecharegistro_planeada_en_proceso
on ocn.intervencion (fecharegistro)
include (idtipointervencion, idobservacionproblematica, idrecurso, fechacompletado, estado)
where estado in ('Planeada', 'EnProceso')";

    // CASO 2: limpieza.
    private const string SqlCaso2SoltarIndice = @"
drop index if exists ocn.intervencion.idx_intervencion_estado_fecharegistro_planeada_en_proceso";

    // CASO 3: consulta por duración (columna calculada persistida).
    private const string SqlCaso3Consulta = @"
select [idintervencion], [idtipointervencion], [idobservacionproblematica], [idrecurso], [fecharegistro], [fechacompletado], duracion, [estado]
from ocn.intervencion i
where duracion > 30";

    // CASO 3: la app agrega la columna calculada solo si no existe (y la elimina al final).
    private const string SqlCaso3AgregarColumna = @"
alter table ocn.intervencion
add duracion as datediff(day, [fecharegistro], [fechacompletado]) persisted";

    // CASO 3: ¿ya existía la columna calculada 'duracion'?
    private const string SqlCaso3ColumnaExiste = @"
select count(*) from sys.computed_columns
where object_id = object_id('ocn.intervencion') and name = 'duracion'";

    // CASO 3: índice sobre la columna calculada.
    private const string SqlCaso3CrearIndice = @"
create nonclustered index idx_intervencion_duracion_computed
on ocn.intervencion (duracion)
include (idtipointervencion, idobservacionproblematica, idrecurso, fecharegistro, fechacompletado, estado)";

    // CASO 3: limpiezas (índice y columna, esta última solo si la app la agregó).
    private const string SqlCaso3SoltarIndice = @"
drop index if exists ocn.intervencion.idx_intervencion_duracion_computed";

    private const string SqlCaso3SoltarColumna = @"
alter table ocn.intervencion drop column if exists duracion";

    // Prefijo común de las corridas: activa las capturas en el MISMO lote que la consulta.
    private const string SqlPrefijoEstadisticas = @"
set statistics io on;
set statistics time on;
set statistics xml on;
";

    // Valor del predicado del caso 1 (idzona = 1) para la lectura del histograma.
    private const double ZonaBuscada = 1;

    private const string NombreTablaCaso2 = "intervencion";
    private const string NombreTablaCaso3 = "intervencion";

    /// <summary>Paso del histograma con los valores crudos necesarios para la lectura automática.</summary>
    private sealed record PasoHistograma(string ClaveRango, double? FilasIguales, double? FilasRango, double? FilasPromedioRango);

    // ============================================================
    // Entradas públicas (una por caso)
    // ============================================================

    /// <summary>
    /// CASO 1 (tratamiento completo): consulta ANTES -> índice compuesto ->
    /// DBCC SHOW_STATISTICS (encabezado, densidades e histograma) -> consulta DESPUÉS
    /// -> drop del índice en finally. Devuelve todo capturado para la vista.
    /// </summary>
    public static async Task<Caso1Resultado> EjecutarCaso1Async(string cadenaConexion)
    {
        var resultado = new Caso1Resultado
        {
            NombreIndice = "idx_levantamiento_idzona_fechalevantamiento",
            DefinicionIndice = SqlCaso1CrearIndice.Trim(),
        };
        var pasos = resultado.Pasos;
        SqlConnection? conexion = null;
        var abierta = false;
        try
        {
            conexion = new SqlConnection(cadenaConexion);
            await conexion.OpenAsync();
            abierta = true;
            pasos.Add("OK: conexión abierta contra la base proyecto_ocn (captura de mensajes activada).");

            // Limpieza previa: si una corrida anterior (o el guion manual) dejó el índice,
            // se elimina para que la corrida ANTES sea honesta.
            await LimpiarAsync(conexion, SqlCaso1SoltarIndice, pasos,
                "índice residual de corridas anteriores eliminado (si existía)");

            resultado.Antes = await EjecutarConsultaAsync(conexion, SqlCaso1Consulta, "ANTES (sin índice)");
            pasos.Add($"OK: consulta ANTES ejecutada ({resultado.Antes.TablaDatos.Filas.Count} filas, {resultado.Antes.DuracionMs} ms).");

            await EjecutarDdlAsync(conexion, SqlCaso1CrearIndice, pasos,
                "índice compuesto idx_levantamiento_idzona_fechalevantamiento creado (sin USING btree, sintaxis inválida en SQL Server)");

            // DBCC SHOW_STATISTICS devuelve 3 conjuntos: STAT_HEADER, DENSITY_VECTOR y HISTOGRAM.
            var (encabezado, densidades, histograma, pasosHistograma) = await LeerEstadisticasAsync(conexion);
            resultado.Encabezado = encabezado;
            resultado.Densidades = densidades;
            resultado.Histograma = histograma;
            resultado.LecturaHistograma = ConstruirLecturaHistograma(pasosHistograma);
            pasos.Add("OK: DBCC SHOW_STATISTICS leído (encabezado, densidades e histograma).");

            resultado.Despues = await EjecutarConsultaAsync(conexion, SqlCaso1Consulta, "DESPUÉS (con índice)");
            pasos.Add($"OK: consulta DESPUÉS ejecutada ({resultado.Despues.TablaDatos.Filas.Count} filas, {resultado.Despues.DuracionMs} ms).");

            resultado.Exito = true;
        }
        catch (Exception ex)
        {
            resultado.Error = ex.Message;
            if (pasos.Count == 0 || !pasos[^1].StartsWith("ERROR:", StringComparison.Ordinal))
                pasos.Add($"ERROR: {ex.Message}");
        }
        finally
        {
            if (abierta && conexion != null)
            {
                // La base debe quedar como estaba: el índice se elimina pase lo que pase.
                await LimpiarAsync(conexion, SqlCaso1SoltarIndice, pasos,
                    "índice idx_levantamiento_idzona_fechalevantamiento eliminado; la base queda como estaba");
                await conexion.DisposeAsync();
            }
        }
        return resultado;
    }

    /// <summary>
    /// CASO 2 (tratamiento simple): consulta ANTES -> índice filtrado -> consulta DESPUÉS
    /// -> drop del índice en finally. Incluye veredicto automático sobre si el optimizador
    /// realmente usó el índice filtrado (requisito 3 de la tarea).
    /// </summary>
    public static async Task<CasoComparacionResultado> EjecutarCaso2Async(string cadenaConexion)
    {
        var resultado = new CasoComparacionResultado
        {
            NombreIndice = "idx_intervencion_estado_fecharegistro_planeada_en_proceso",
            DefinicionIndice = SqlCaso2CrearIndice.Trim(),
        };
        await EjecutarCicloSimpleAsync(cadenaConexion, resultado, SqlCaso2Consulta, SqlCaso2CrearIndice, SqlCaso2SoltarIndice,
            NombreTablaCaso2, "índice filtrado");
        return resultado;
    }

    /// <summary>
    /// CASO 3 (tratamiento simple + gestión de la columna calculada): verifica/agrega la
    /// columna calculada persistida 'duracion' -> consulta ANTES -> índice sobre la columna
    /// calculada -> consulta DESPUÉS -> drop del índice y de la columna (solo si la app la
    /// agregó) en finally.
    /// </summary>
    public static async Task<CasoComparacionResultado> EjecutarCaso3Async(string cadenaConexion)
    {
        var resultado = new CasoComparacionResultado
        {
            NombreIndice = "idx_intervencion_duracion_computed",
            DefinicionIndice = SqlCaso3CrearIndice.Trim(),
        };
        var pasos = resultado.Pasos;
        SqlConnection? conexion = null;
        var abierta = false;
        try
        {
            conexion = new SqlConnection(cadenaConexion);
            await conexion.OpenAsync();
            abierta = true;
            pasos.Add("OK: conexión abierta contra la base proyecto_ocn (captura de mensajes activada).");

            // Limpieza previa del índice para que la corrida ANTES sea honesta.
            await LimpiarAsync(conexion, SqlCaso3SoltarIndice, pasos,
                "índice residual de corridas anteriores eliminado (si existía)");

            // ¿La columna calculada ya existía? Si no, la app la agrega (y la elimina al final).
            var columnaExiste = await ExisteColumnaDuracionAsync(conexion);
            if (columnaExiste)
            {
                pasos.Add("OK: la columna calculada 'duracion' ya existía en ocn.intervencion (creada por el estudiante); NO se eliminará al final.");
            }
            else
            {
                await EjecutarDdlAsync(conexion, SqlCaso3AgregarColumna, pasos,
                    "columna calculada 'duracion' AGREGADA temporalmente (persisted); se eliminará al final de la corrida");
                resultado.ColumnaCalculadaAgregadaYEliminada = true;
            }

            resultado.Antes = await EjecutarConsultaAsync(conexion, SqlCaso3Consulta, "ANTES (sin índice)");
            pasos.Add($"OK: consulta ANTES ejecutada ({resultado.Antes.TablaDatos.Filas.Count} filas, {resultado.Antes.DuracionMs} ms).");

            await EjecutarDdlAsync(conexion, SqlCaso3CrearIndice, pasos,
                "índice idx_intervencion_duracion_computed creado sobre la columna calculada");

            resultado.Despues = await EjecutarConsultaAsync(conexion, SqlCaso3Consulta, "DESPUÉS (con índice)");
            pasos.Add($"OK: consulta DESPUÉS ejecutada ({resultado.Despues.TablaDatos.Filas.Count} filas, {resultado.Despues.DuracionMs} ms).");

            CalcularVeredicto(resultado, NombreTablaCaso3);
            resultado.Exito = true;
        }
        catch (Exception ex)
        {
            resultado.Error = ex.Message;
            if (pasos.Count == 0 || !pasos[^1].StartsWith("ERROR:", StringComparison.Ordinal))
                pasos.Add($"ERROR: {ex.Message}");
        }
        finally
        {
            if (abierta && conexion != null)
            {
                // La base debe quedar como estaba: índice siempre, columna solo si la app la agregó.
                await LimpiarAsync(conexion, SqlCaso3SoltarIndice, pasos,
                    "índice idx_intervencion_duracion_computed eliminado");
                if (resultado.ColumnaCalculadaAgregadaYEliminada)
                {
                    await LimpiarAsync(conexion, SqlCaso3SoltarColumna, pasos,
                        "columna calculada 'duracion' eliminada (solo porque esta corrida la agregó); la base queda como estaba");
                }
                await conexion.DisposeAsync();
            }
        }
        return resultado;
    }

    // ============================================================
    // Ciclo simple compartido por el caso 2 (y base del 3)
    // ============================================================

    /// <summary>Ciclo ANTES -> DDL -> DESPUÉS con limpieza en finally y veredicto automático.</summary>
    private static async Task EjecutarCicloSimpleAsync(string cadenaConexion, CasoComparacionResultado resultado,
        string sqlConsulta, string sqlCrear, string sqlSoltar, string tabla, string tipoIndice)
    {
        var pasos = resultado.Pasos;
        SqlConnection? conexion = null;
        var abierta = false;
        try
        {
            conexion = new SqlConnection(cadenaConexion);
            await conexion.OpenAsync();
            abierta = true;
            pasos.Add("OK: conexión abierta contra la base proyecto_ocn (captura de mensajes activada).");

            // Limpieza previa: evita que un índice residual disimule la mejora.
            await LimpiarAsync(conexion, sqlSoltar, pasos, "índice residual de corridas anteriores eliminado (si existía)");

            resultado.Antes = await EjecutarConsultaAsync(conexion, sqlConsulta, "ANTES (sin índice)");
            pasos.Add($"OK: consulta ANTES ejecutada ({resultado.Antes.TablaDatos.Filas.Count} filas, {resultado.Antes.DuracionMs} ms).");

            await EjecutarDdlAsync(conexion, sqlCrear, pasos, $"{tipoIndice} {resultado.NombreIndice} creado");

            resultado.Despues = await EjecutarConsultaAsync(conexion, sqlConsulta, "DESPUÉS (con índice)");
            pasos.Add($"OK: consulta DESPUÉS ejecutada ({resultado.Despues.TablaDatos.Filas.Count} filas, {resultado.Despues.DuracionMs} ms).");

            CalcularVeredicto(resultado, tabla);
            resultado.Exito = true;
        }
        catch (Exception ex)
        {
            resultado.Error = ex.Message;
            if (pasos.Count == 0 || !pasos[^1].StartsWith("ERROR:", StringComparison.Ordinal))
                pasos.Add($"ERROR: {ex.Message}");
        }
        finally
        {
            if (abierta && conexion != null)
            {
                // La base debe quedar como estaba: el índice se elimina pase lo que pase.
                await LimpiarAsync(conexion, sqlSoltar, pasos,
                    $"índice {resultado.NombreIndice} eliminado; la base queda como estaba");
                await conexion.DisposeAsync();
            }
        }
    }

    // ============================================================
    // Captura de una corrida (STATISTICS IO/TIME/XML) y utilidades SQL
    // ============================================================

    /// <summary>
    /// Ejecuta la consulta con STATISTICS IO/TIME/XML en un solo lote, lee la rejilla,
    /// captura los mensajes informativos y el plan real (showplan XML).
    /// </summary>
    private static async Task<ResultadoEjecucion> EjecutarConsultaAsync(SqlConnection conexion, string consulta, string titulo)
    {
        var resultado = new ResultadoEjecucion { Titulo = titulo, IdPlanCache = Guid.NewGuid() };
        var cronometro = Stopwatch.StartNew();
        var captor = new CaptorMensajes();
        captor.Adjuntar(conexion);
        try
        {
            using var comando = conexion.CreateCommand();
            comando.CommandTimeout = 120;
            comando.CommandText = SqlPrefijoEstadisticas + consulta;
            using var lector = await comando.ExecuteReaderAsync();

            // Rejilla principal de datos.
            resultado.TablaDatos = await LeerTablaAsync(lector);

            // Conjuntos adicionales: con STATISTICS XML el plan real llega como
            // resultado extra de una sola columna XML.
            while (await lector.NextResultAsync())
            {
                if (lector.FieldCount == 1 && await lector.ReadAsync())
                {
                    var valor = lector.GetValue(0);
                    var xml = valor switch
                    {
                        null or DBNull => null,
                        SqlXml sqlXml => sqlXml.Value,
                        _ => valor.ToString(),
                    };
                    if (!string.IsNullOrWhiteSpace(xml) && xml!.TrimStart().StartsWith('<'))
                        resultado.PlanXml = xml;
                }
            }
        }
        finally
        {
            captor.Detach(conexion);
            cronometro.Stop();
            resultado.DuracionMs = cronometro.ElapsedMilliseconds;
            ClasificarMensajes(captor.TomarMensajes(), resultado);
        }
        resultado.Plan = ParsearPlan(resultado.PlanXml);
        return resultado;
    }

    /// <summary>Captura los mensajes que SQL Server envía por la conexión (STATISTICS IO/TIME).</summary>
    private sealed class CaptorMensajes
    {
        private readonly object _bloqueo = new();
        private readonly List<string> _mensajes = new();

        public void Adjuntar(SqlConnection conexion)
        {
            conexion.InfoMessage += AlRecibirMensaje;
        }

        public void Detach(SqlConnection conexion)
        {
            conexion.InfoMessage -= AlRecibirMensaje;
        }

        private void AlRecibirMensaje(object sender, SqlInfoMessageEventArgs e)
        {
            lock (_bloqueo)
            {
                foreach (SqlError error in e.Errors)
                    _mensajes.Add(error.Message);
            }
        }

        /// <summary>Devuelve los mensajes capturados y vacía el acopiador.</summary>
        public List<string> TomarMensajes()
        {
            lock (_bloqueo)
            {
                var copia = new List<string>(_mensajes);
                _mensajes.Clear();
                return copia;
            }
        }
    }

    /// <summary>
    /// Separa los mensajes capturados: lecturas lógicas/físicas -> IO,
    /// CPU/tiempo transcurrido -> TIME, el resto -> lista general.
    /// (Se reconocen también los textos en español por si la instancia no está en inglés.)
    /// </summary>
    private static void ClasificarMensajes(List<string> mensajes, ResultadoEjecucion resultado)
    {
        foreach (var mensaje in mensajes)
        {
            if (mensaje.Contains("logical reads", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("lecturas lógicas", StringComparison.OrdinalIgnoreCase))
                resultado.MensajesIo.Add(mensaje);
            else if (mensaje.Contains("CPU time", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("elapsed time", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("tiempo de CPU", StringComparison.OrdinalIgnoreCase)
                || mensaje.Contains("tiempo transcurrido", StringComparison.OrdinalIgnoreCase))
                resultado.MensajesTiempo.Add(mensaje);
            else
                resultado.MensajesOtros.Add(mensaje);
        }
    }

    /// <summary>Ejecuta un DDL registrando el paso; en error deja constancia y propaga la excepción.</summary>
    private static async Task EjecutarDdlAsync(SqlConnection conexion, string ddl, List<string> pasos, string descripcion)
    {
        try
        {
            using var comando = conexion.CreateCommand();
            comando.CommandTimeout = 120;
            comando.CommandText = ddl;
            await comando.ExecuteNonQueryAsync();
            pasos.Add($"OK: {descripcion}.");
        }
        catch (Exception ex)
        {
            pasos.Add($"ERROR: {descripcion} -> {ex.Message}");
            throw;
        }
    }

    /// <summary>Ejecuta un DDL de limpieza sin propagar errores (deja constancia en la bitácora).</summary>
    private static async Task LimpiarAsync(SqlConnection conexion, string ddl, List<string> pasos, string descripcion)
    {
        try
        {
            using var comando = conexion.CreateCommand();
            comando.CommandTimeout = 120;
            comando.CommandText = ddl;
            await comando.ExecuteNonQueryAsync();
            pasos.Add($"OK (limpieza): {descripcion}.");
        }
        catch (Exception ex)
        {
            pasos.Add($"ERROR en limpieza ({descripcion}): {ex.Message}");
        }
    }

    /// <summary>Lee una rejilla de resultados en el modelo genérico de tabla.</summary>
    private static async Task<TablaDatos> LeerTablaAsync(SqlDataReader lector)
    {
        var tabla = new TablaDatos();
        for (var i = 0; i < lector.FieldCount; i++)
            tabla.Columnas.Add(lector.GetName(i));
        while (await lector.ReadAsync())
        {
            var fila = new string[lector.FieldCount];
            for (var i = 0; i < lector.FieldCount; i++)
                fila[i] = FormatearCelda(lector.GetValue(i));
            tabla.Filas.Add(fila);
        }
        return tabla;
    }

    /// <summary>
    /// Lee el histograma capturando además los valores crudos de los pasos
    /// (RANGE_HI_KEY, EQ_ROWS, RANGE_ROWS, AVG_RANGE_ROWS) para la lectura automática.
    /// </summary>
    private static async Task<(TablaDatos Tabla, List<PasoHistograma> Pasos)> LeerHistogramaAsync(SqlDataReader lector)
    {
        var tabla = await LeerTablaAsync(lector);

        // Mapa de columnas del histograma (defensivo: si cambia el formato, se ignora).
        var mapa = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < lector.FieldCount; i++)
            mapa[lector.GetName(i)] = i;
        var completo = mapa.ContainsKey("RANGE_HI_KEY") && mapa.ContainsKey("EQ_ROWS")
            && mapa.ContainsKey("RANGE_ROWS") && mapa.ContainsKey("AVG_RANGE_ROWS");

        var pasos = new List<PasoHistograma>();
        if (!completo)
            return (tabla, pasos);

        foreach (var fila in tabla.Filas)
        {
            pasos.Add(new PasoHistograma(
                fila[mapa["RANGE_HI_KEY"]],
                ADoble(fila[mapa["EQ_ROWS"]]),
                ADoble(fila[mapa["RANGE_ROWS"]]),
                ADoble(fila[mapa["AVG_RANGE_ROWS"]])));
        }
        return (tabla, pasos);
    }

    /// <summary>Ejecuta DBCC SHOW_STATISTICS y devuelve sus 3 conjuntos de resultados.</summary>
    private static async Task<(TablaDatos Encabezado, TablaDatos Densidades, TablaDatos Histograma, List<PasoHistograma> Pasos)>
        LeerEstadisticasAsync(SqlConnection conexion)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandTimeout = 120;
        comando.CommandText = SqlCaso1Dbcc;
        using var lector = await comando.ExecuteReaderAsync();

        var encabezado = await LeerTablaAsync(lector);
        if (!await lector.NextResultAsync())
            return (encabezado, new TablaDatos(), new TablaDatos(), new List<PasoHistograma>());

        var densidades = await LeerTablaAsync(lector);
        if (!await lector.NextResultAsync())
            return (encabezado, densidades, new TablaDatos(), new List<PasoHistograma>());

        var (histograma, pasos) = await LeerHistogramaAsync(lector);
        return (encabezado, densidades, histograma, pasos);
    }

    /// <summary>Indica si la columna calculada 'duracion' ya existe en ocn.intervencion.</summary>
    private static async Task<bool> ExisteColumnaDuracionAsync(SqlConnection conexion)
    {
        using var comando = conexion.CreateCommand();
        comando.CommandText = SqlCaso3ColumnaExiste;
        var valor = await comando.ExecuteScalarAsync();
        return Convert.ToInt32(valor, CultureInfo.InvariantCulture) > 0;
    }

    // ============================================================
    // Lectura automática del histograma (caso 1)
    // ============================================================

    /// <summary>
    /// Construye el párrafo de lectura del histograma para idzona = 1:
    /// paso exacto (EQ_ROWS) o rango previo (AVG_RANGE_ROWS) + la justificación
    /// del índice compuesto cubriente.
    /// </summary>
    private static string ConstruirLecturaHistograma(List<PasoHistograma> pasos)
    {
        // 1) Paso exacto: el histograma tiene RANGE_HI_KEY = 1.
        var exacto = pasos.FirstOrDefault(p => ADoble(p.ClaveRango) == ZonaBuscada);
        if (exacto != null)
        {
            return "Lectura automática del histograma: existe un paso EXACTO con RANGE_HI_KEY = 1 (la zona buscada) " +
                $"y el histograma estima EQ_ROWS = {FormatearNumero(exacto.FilasIguales)} filas para idzona = 1. " + ExplicacionIndiceCompuesto;
        }

        // 2) Paso previo: idzona = 1 cae dentro del rango abierto que inicia en el último paso menor a 1.
        PasoHistograma? previo = null;
        foreach (var paso in pasos)
        {
            var clave = ADoble(paso.ClaveRango);
            if (clave.HasValue && clave.Value < ZonaBuscada)
                previo = paso;
        }
        if (previo != null)
        {
            return "Lectura automática del histograma: no hay un paso exacto para idzona = 1; el valor cae dentro del " +
                $"rango abierto que inicia en RANGE_HI_KEY = {previo.ClaveRango}, donde el histograma estima " +
                $"AVG_RANGE_ROWS = {FormatearNumero(previo.FilasPromedioRango)} filas promedio por valor " +
                $"({FormatearNumero(previo.FilasRango)} filas repartidas en ese rango). " + ExplicacionIndiceCompuesto;
        }

        // 3) Sin cobertura: la zona podría no existir o la estadística está vacía; se mantiene la explicación general.
        return "Lectura automática del histograma: no se encontró un paso que cubra idzona = 1 " +
            "(la zona podría no existir en la tabla o la estadística podría no tener pasos). " + ExplicacionIndiceCompuesto;
    }

    private const string ExplicacionIndiceCompuesto =
        "Esto respalda el índice compuesto (idzona, fechalevantamiento): la igualdad sobre idzona coincide con la " +
        "primera clave, así que el optimizador puede posicionar un Index Seek con una estimación de selectividad " +
        "apoyada en el histograma; la segunda clave (fechalevantamiento) mantiene las entradas de cada zona ordenadas " +
        "por fecha y satisface el ORDER BY ... DESC sin operador Sort adicional; las demás columnas van en INCLUDE " +
        "para que el índice sea cubriente y, además, idlevantamiento (clave primaria agrupada) viaja implícitamente " +
        "como localizador de fila en cada entrada de un índice no agrupado, por lo que no se requiere Key Lookup.";

    // ============================================================
    // Parseo del showplan XML y veredicto automático
    // ============================================================

    /// <summary>
    /// Convierte el showplan XML en un resumen legible. Defensivo: si el XML falta
    /// o no se puede interpretar, devuelve null sin romper la página.
    /// </summary>
    private static PlanResumen? ParsearPlan(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return null;
        try
        {
            var documento = XDocument.Parse(xml);
            var ns = documento.Root?.Name.Namespace ?? XNamespace.None;
            // El lote solo contiene un SELECT; se toma el último StmtSimple por seguridad.
            var stmt = documento.Descendants(ns + "StmtSimple").LastOrDefault();
            if (stmt == null)
                return null;

            var resumen = new PlanResumen
            {
                CostoEstimado = ADoble((string?)stmt.Attribute("StatementSubTreeCost"))
                    ?? ADoble((string?)stmt.Attribute("EstimatedTotalSubtreeCost")) ?? 0,
                FilasEstimadas = ADoble((string?)stmt.Attribute("StatementEstRows"))
                    ?? ADoble((string?)stmt.Attribute("StatementEstimateRows")) ?? 0,
                NivelOptimizacion = (string?)stmt.Attribute("StatementOptmLevel") ?? "",
            };

            var queryPlan = stmt.Element(ns + "QueryPlan");
            resumen.GradoParalelismo = (int?)queryPlan?.Attribute("DegreeOfParallelism") ?? 1;

            foreach (var relOp in queryPlan?.Descendants(ns + "RelOp") ?? Enumerable.Empty<XElement>())
            {
                resumen.Operadores.Add(new OperadorPlan
                {
                    OperadorFisico = (string?)relOp.Attribute("PhysicalOp") ?? "",
                    OperadorLogico = (string?)relOp.Attribute("LogicalOp") ?? "",
                    Objeto = LeerObjeto(relOp, ns),
                    CostoSubarbol = ADoble((string?)relOp.Attribute("EstimatedTotalSubtreeCost")) ?? 0,
                    FilasEstimadas = ADoble((string?)relOp.Attribute("EstimateRows")) ?? 0,
                    FilasReales = ADoble((string?)relOp.Attribute("ActualRows")) ?? 0,
                    FilasRealesDisponibles = ADoble((string?)relOp.Attribute("ActualRows")).HasValue,
                });
            }

            foreach (var advertencia in queryPlan?.Descendants(ns + "Warnings").Elements() ?? Enumerable.Empty<XElement>())
            {
                var atributos = advertencia.Attributes()
                    .Where(a => !string.IsNullOrEmpty(a.Value))
                    .Select(a => $"{a.Name.LocalName}: {a.Value}");
                resumen.Advertencias.Add(advertencia.Name.LocalName + (advertencia.Attributes().Any() ? " (" + string.Join(", ", atributos) + ")" : ""));
            }
            return resumen;
        }
        catch
        {
            // Defensivo: un plan ilegible no debe tumbar la página de resultados.
            return null;
        }
    }

    /// <summary>Construye la referencia "esquema.tabla [índice]" del Object de un RelOp.</summary>
    private static string LeerObjeto(XElement relOp, XNamespace ns)
    {
        var objeto = relOp.Descendants(ns + "Object").FirstOrDefault();
        if (objeto == null)
            return "";
        var partes = new List<string>();
        var esquema = (string?)objeto.Attribute("Schema");
        var tabla = (string?)objeto.Attribute("Table");
        var indice = (string?)objeto.Attribute("Index");
        if (!string.IsNullOrEmpty(esquema) && !string.IsNullOrEmpty(tabla))
            partes.Add($"{esquema}.{tabla}");
        else if (!string.IsNullOrEmpty(tabla))
            partes.Add(tabla!);
        if (!string.IsNullOrEmpty(indice))
            partes.Add($"[{indice}]");
        return string.Join(" ", partes);
    }

    /// <summary>
    /// Veredicto automático: revisa si el plan DESPUÉS referencia al índice recién creado
    /// (Index Seek/Index Scan sobre él) o si siguió con un Clustered Index Scan de la tabla.
    /// </summary>
    private static void CalcularVeredicto(CasoComparacionResultado resultado, string tabla)
    {
        var plan = resultado.Despues?.Plan;
        if (plan == null)
        {
            resultado.IndiceUsadoEnDespues = false;
            resultado.NotaAprendizaje = "No se pudo inspeccionar el plan de ejecución real del bloque DESPUÉS " +
                "(el XML no llegó o no se pudo interpretar), así que no hay veredicto automático. " +
                "Aun así puedes comparar los costos estimados, las lecturas lógicas y los tiempos de ambos bloques.";
            return;
        }

        var usado = plan.Operadores.Any(op =>
            (op.OperadorFisico == "Index Seek" || op.OperadorFisico == "Index Scan")
            && op.Objeto.Contains(resultado.NombreIndice, StringComparison.OrdinalIgnoreCase));
        resultado.IndiceUsadoEnDespues = usado;

        if (usado)
            return;

        // No lo usó: esto es evidencia válida de aprendizaje (requisito 3 de la tarea).
        var escaneoAgrupado = plan.Operadores.Any(op =>
            op.OperadorFisico == "Clustered Index Scan"
            && op.Objeto.Contains(tabla, StringComparison.OrdinalIgnoreCase));
        var detalle = escaneoAgrupado
            ? "el plan siguió con un Clustered Index Scan sobre la tabla"
            : "el plan resolvió la consulta con otros operadores";
        resultado.NotaAprendizaje = $"El plan DESPUÉS no hace referencia al índice '{resultado.NombreIndice}': {detalle}. " +
            "Esto es evidencia válida de aprendizaje para el requisito 3 de la tarea ('si no mejoró como esperaban'): " +
            "el optimizador puede preferir el escaneo cuando la tabla es pequeña y leerla completa es más barato que " +
            "buscar por el índice, cuando estima una selectividad distinta a la real, o cuando las estadísticas del " +
            "índice recién creado aún no son representativas. Compara el costo estimado y las lecturas lógicas de ambos " +
            "bloques para explicar en tu reporte por qué tomó esa decisión.";
    }

    // ============================================================
    // Formato de celdas y números
    // ============================================================

    /// <summary>Serializa una celda SQL a texto de presentación.</summary>
    private static string FormatearCelda(object? valor) => valor switch
    {
        null or DBNull => "NULL",
        DateTime fecha => fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
        bool bandera => bandera ? "Sí" : "No",
        IFormattable formateable => formateable.ToString(null, CultureInfo.InvariantCulture),
        _ => valor.ToString() ?? "",
    };

    /// <summary>Convierte texto a double (cultura invariante); null si no se puede.</summary>
    private static double? ADoble(string? texto)
        => double.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor) ? valor : null;

    /// <summary>Formatea un número para la lectura del histograma (sin arrastre de decimales).</summary>
    private static string FormatearNumero(double? valor)
        => valor?.ToString("0.######", CultureInfo.InvariantCulture) ?? "?";
}
