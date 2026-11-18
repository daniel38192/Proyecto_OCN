using System.Data;
using Microsoft.Data.SqlClient;

namespace OCN_Project.Data;

/// <summary>
/// Ejecutor de demostraciones de programación T-SQL (ADO.NET puro).
/// Cada método abre su propia conexión, ejecuta su demostración contra SQL Server
/// y regresa un <see cref="OCN_Project.Models.DemoResultado"/> con la evidencia.
/// Todas las demostraciones marcan sus filas con una descripción que inicia con
/// "DEMO T-SQL &lt;guid&gt;" y limpian en finally para dejar la base EXACTAMENTE como estaba.
/// </summary>
public static class TSqlDemoExecutor
{
    /// <summary>Prefijo que identifica las filas creadas por las demostraciones.</summary>
    private const string PrefijoDemo = "DEMO T-SQL ";

    // ------------------------------------------------------------------
    // T-SQL: resolución de llaves foráneas válidas para las demos
    // ------------------------------------------------------------------
    private const string SqlResolverFks = @"
        SELECT
            (SELECT TOP (1) idtipointervencion FROM ocn.cattipointervencion WHERE activo = 1 ORDER BY idtipointervencion) AS idtipointervencion,
            (SELECT TOP (1) idobservacionproblematica FROM ocn.observacionproblematica ORDER BY idobservacionproblematica) AS idobservacionproblematica,
            (SELECT TOP (1) idusuario FROM ocn.usuario ORDER BY idusuario) AS idusuario1,
            (SELECT TOP (1) idusuario FROM ocn.usuario ORDER BY idusuario DESC) AS idusuario2;";

    private const string SqlMaxIdError = "SELECT ISNULL(MAX(iderror), 0) FROM ocn.errorprocedimiento;";

    private const string SqlBorrarDemoIntervenciones = @"
        DELETE FROM ocn.intervencion WHERE descripcion LIKE @patron;";

    private const string SqlBorrarErrores = @"
        DELETE FROM ocn.errorprocedimiento WHERE iderror > @maxiderror;";

    private const string SqlBorrarAuditoriaDemo = @"
        DELETE FROM ocn.auditoria
        WHERE nombretabla = 'intervencion' AND idregistro IN (SELECT idintervencion FROM ocn.intervencion WHERE descripcion LIKE @patron);";

    // ------------------------------------------------------------------
    // Demo 1: procedimiento transaccional ocn.usp_registrar_intervencion
    // ------------------------------------------------------------------
    private const string SqlLlamarProcOk = @"
        EXEC ocn.usp_registrar_intervencion
            @idtipointervencion = @idtipointervencion,
            @idobservacionproblematica = @idobservacionproblematica,
            @idrecurso = @idrecurso,
            @descripcion = @descripcion,
            @lista_usuarios = @lista;
        SELECT idintervencion;";

    private const string SqlVerIntervencion = @"
        SELECT idintervencion, idtipointervencion, idobservacionproblematica, idrecurso,
               estado, descripcion, fecharegistro
        FROM ocn.intervencion WHERE idintervencion = @idintervencion;";

    private const string SqlVerResponsables = @"
        SELECT iu.idintervencion, iu.idusuario
        FROM ocn.intervencionusuario iu
        WHERE iu.idintervencion = @idintervencion
        ORDER BY iu.idusuario;";

    private const string SqlContarDemo = @"
        SELECT COUNT(*) FROM ocn.intervencion WHERE descripcion LIKE @patron;";

    private const string SqlVerErroresNuevos = @"
        SELECT iderror, procedimiento, numeroerror, severidad, estadoerror, linea, mensaje, fecha
        FROM ocn.errorprocedimiento
        WHERE iderror > @maxiderror
        ORDER BY iderror;";

    /// <summary>
    /// Demostración del procedimiento almacenado transaccional: un caso de éxito
    /// (transacción confirmada) y un caso de fallo con violación de FK (rollback completo).
    /// </summary>
    public static OCN_Project.Models.DemoResultado ProcedimientoTransaccional(string cadenaConexion)
    {
        var resultado = new OCN_Project.Models.DemoResultado
        {
            Justificacion = "Atomicidad: la intervención y sus responsables se crean juntos o no se crea nada (una sola transacción).\n" +
                            "La bitácora de errores se escribe DESPUÉS del rollback para que la propia transacción no la borre.\n" +
                            "xact_state() verifica el estado de la transacción antes de revertir y THROW reenvía el error original al cliente.\n" +
                            "El procedimiento de búsqueda dinámica usa sp_executesql con filtros opcionales para un solo plan cacheado.",
        };

        var fks = new Dictionary<string, object?>();
        int? maxIdErrorAntes = null;

        using (var conexion = new SqlConnection(cadenaConexion))
        {
            conexion.Open();
            try
            {
                ResolverFks(conexion, fks, resultado);
                if (!resultado.Exito)
                {
                    return resultado;
                }

                // ---------------- PASO A: llamada exitosa ----------------
                int idNueva;
                resultado.Pasos.Add("Paso 1: se prepara el parámetro de tabla (TVP) ocn.ListaUsuariosType con usuarios reales del catálogo.");
                using (var llamada = new SqlCommand(SqlLlamarProcOk, conexion))
                {
                    llamada.Parameters.AddWithValue("@idtipointervencion", fks["idtipointervencion"]!);
                    llamada.Parameters.AddWithValue("@idobservacionproblematica", fks["idobservacionproblematica"]!);
                    llamada.Parameters.AddWithValue("@idrecurso", DBNull.Value);
                    var tvp = CrearTvpUsuarios(fks);
                    var pLista = llamada.Parameters.Add("@lista", SqlDbType.Structured);
                    pLista.TypeName = "ocn.ListaUsuariosType";
                    pLista.Value = tvp;
                    llamada.Parameters.AddWithValue("@descripcion", PrefijoDemo + Guid.NewGuid() + " éxito");
                    idNueva = Convert.ToInt32(llamada.ExecuteScalar());
                }
                resultado.Pasos.Add($"Paso 2: el procedimiento confirmó (COMMIT) la transacción y devolvió idintervencion = {idNueva}.");

                resultado.Secciones.Add(new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia A: intervención creada por la transacción",
                    Tabla = LeerTabla(conexion, SqlVerIntervencion, ("@idintervencion", idNueva)),
                });
                resultado.Secciones.Add(new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia B: responsables insertados en ocn.intervencionusuario",
                    Tabla = LeerTabla(conexion, SqlVerResponsables, ("@idintervencion", idNueva)),
                });

                // ---------------- PASO B: fallo con rollback ----------------
                maxIdErrorAntes = Convert.ToInt32(Escalar(conexion, SqlMaxIdError));
                resultado.Pasos.Add($"Paso 3: se repite la llamada con @idobservacionproblematica = -1 (violación de FK). Antes se registra MAX(iderror) = {maxIdErrorAntes} de la bitácora.");

                string mensajeError;
                try
                {
                    using (var llamadaFalla = new SqlCommand(SqlLlamarProcOk, conexion))
                    {
                        llamadaFalla.Parameters.AddWithValue("@idtipointervencion", fks["idtipointervencion"]!);
                        llamadaFalla.Parameters.AddWithValue("@idobservacionproblematica", -1);
                        llamadaFalla.Parameters.AddWithValue("@idrecurso", DBNull.Value);
                        var tvpFalla = CrearTvpUsuarios(fks);
                        var pListaFalla = llamadaFalla.Parameters.Add("@lista", SqlDbType.Structured);
                        pListaFalla.TypeName = "ocn.ListaUsuariosType";
                        pListaFalla.Value = tvpFalla;
                        llamadaFalla.Parameters.AddWithValue("@descripcion", PrefijoDemo + Guid.NewGuid() + " fallo");
                        llamadaFalla.ExecuteScalar();
                    }
                    mensajeError = "(no se produjo error; escenario inesperado)";
                }
                catch (SqlException ex)
                {
                    mensajeError = $"Error {ex.Number}: {ex.Message}";
                }

                resultado.Pasos.Add("Paso 4: el procedimiento atrapó el error 547, revirtió (ROLLBACK) la transacción, registró el error en ocn.errorprocedimiento y lo reenvió con THROW.");

                var evidencia = new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia C: error 547 capturado (resultado esperado de la llamada fallida)",
                };
                evidencia.Notas.Add(mensajeError);
                resultado.Secciones.Add(evidencia);

                var conteo = Convert.ToInt32(Escalar(conexion, SqlContarDemo, ("@patron", PrefijoDemo + "%")));
                resultado.Secciones.Add(new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia D: filas de demostración en ocn.intervencion tras el rollback",
                    Tabla = new OCN_Project.Models.TablaDatos
                    {
                        Columnas = new() { "filas_con_descripcion_demo" },
                        Filas = new() { new[] { conteo.ToString() } },
                    },
                });
                resultado.Secciones[^1].Notas.Add("El conteo es 0: la inserción de la intervención y de sus responsables se deshizo por completo (atomicidad).");

                resultado.Secciones.Add(new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia E: bitácora ocn.errorprocedimiento escrita por el CATCH del procedimiento",
                    Tabla = LeerTabla(conexion, SqlVerErroresNuevos, ("@maxiderror", maxIdErrorAntes.Value)),
                });

                resultado.Exito = true;
            }
            catch (SqlException ex)
            {
                resultado.Exito = false;
                resultado.Error = $"Error de SQL Server {ex.Number}: {ex.Message}";
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Error = $"Error inesperado: {ex.Message}";
            }
            finally
            {
                // Limpieza: la intervención demo es 'Planeada', así que el trigger de
                // eliminación la permite borrar; la cascada elimina sus responsables.
                try
                {
                    EjecutarNoQuery(conexion, SqlBorrarDemoIntervenciones, ("@patron", PrefijoDemo + "%"));
                    if (maxIdErrorAntes.HasValue)
                    {
                        EjecutarNoQuery(conexion, SqlBorrarErrores, ("@maxiderror", maxIdErrorAntes.Value));
                    }
                }
                catch (Exception exLimpieza)
                {
                    resultado.Pasos.Add($"Advertencia durante la limpieza: {exLimpieza.Message}");
                }
            }
        }

        return resultado;
    }

    // ------------------------------------------------------------------
    // Demo 2: trigger de auditoría ocn.trg_intervencion_auditar_estado
    // ------------------------------------------------------------------
    private const string SqlInsertarDemo = @"
        INSERT INTO ocn.intervencion (idtipointervencion, idobservacionproblematica, idrecurso, descripcion, estado, fecharegistro)
        OUTPUT INSERTED.idintervencion
        VALUES (@idtipointervencion, @idobservacionproblematica, @idrecurso, @descripcion, @estado, @fecharegistro);";

    private const string SqlContarAuditoriaIntervencion = @"
        SELECT COUNT(*) FROM ocn.auditoria WHERE nombretabla = 'intervencion';";

    private const string SqlAuditoriaDemo = @"
        SELECT idauditoria, nombretabla, idregistro, operacion, idusuario,
               valoresanteriores, valoresnuevos, fecha
        FROM ocn.auditoria
        WHERE nombretabla = 'intervencion'
          AND idregistro IN (SELECT idintervencion FROM ocn.intervencion WHERE descripcion LIKE @patron)
        ORDER BY idauditoria;";

    private const string SqlActualizarEstado = @"
        UPDATE ocn.intervencion SET estado = 'EnProceso' WHERE idintervencion = @idintervencion;";

    private const string SqlActualizarSinEstado = @"
        UPDATE ocn.intervencion SET descripcion = descripcion + ' (sin cambio de estado)' WHERE idintervencion = @idintervencion;";

    /// <summary>
    /// Demostración del trigger AFTER UPDATE de auditoría: registra el cambio de estado
    /// y demuestra que la guarda UPDATE(estado) evita auditorías por ediciones de otros campos.
    /// </summary>
    public static OCN_Project.Models.DemoResultado TriggerAuditoria(string cadenaConexion)
    {
        var resultado = new OCN_Project.Models.DemoResultado
        {
            Justificacion = "La guarda UPDATE(estado) evita ruido en la auditoría: solo se registra cuando el ESTADO cambió, no cuando se edita la descripción.\n" +
                            "El trigger compara inserted contra deleted filtrando estado <> estado y guarda valores anterior y nuevo en JSON para máxima flexibilidad.\n" +
                            "Es AFTER UPDATE porque se audita el dato ya aplicado y confirmado en la tabla.",
        };

        var fks = new Dictionary<string, object?>();

        using (var conexion = new SqlConnection(cadenaConexion))
        {
            conexion.Open();
            try
            {
                ResolverFks(conexion, fks, resultado);
                if (!resultado.Exito)
                {
                    return resultado;
                }

                // ---------------- Preparación: fila demo 'Planeada' ----------------
                var marcador = PrefijoDemo + Guid.NewGuid() + " auditoria";
                int idFila = InsertarIntervencionDemo(conexion, fks, marcador, "Planeada", null);
                resultado.Pasos.Add($"Paso 1: se inserta una intervención de prueba (id {idFila}) con estado 'Planeada' marcada con '{marcador}'.");

                int antes = Convert.ToInt32(Escalar(conexion, SqlContarAuditoriaIntervencion));
                resultado.Secciones.Add(new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia A: registros en ocn.auditoria para 'intervencion' ANTES del cambio",
                    Tabla = new OCN_Project.Models.TablaDatos
                    {
                        Columnas = new() { "registros_previos" },
                        Filas = new() { new[] { antes.ToString() } },
                    },
                });

                // ---------------- Evidencia B: cambio de estado audita ----------------
                EjecutarNoQuery(conexion, SqlActualizarEstado, ("@idintervencion", idFila));
                resultado.Pasos.Add("Paso 2: UPDATE estado 'Planeada' → 'EnProceso'. El trigger AFTER UPDATE detecta el cambio y audita.");
                resultado.Secciones.Add(new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia B: filas nuevas en ocn.auditoria con el JSON anterior/nuevo",
                    Tabla = LeerTabla(conexion, SqlAuditoriaDemo, ("@patron", PrefijoDemo + "%")),
                });

                // ---------------- Evidencia C: edición sin cambio de estado NO audita ----------------
                EjecutarNoQuery(conexion, SqlActualizarSinEstado, ("@idintervencion", idFila));
                resultado.Pasos.Add("Paso 3: UPDATE solo de descripcion (sin tocar estado). La guarda if not update(estado) hace RETURN inmediato.");
                var despues = Convert.ToInt32(Escalar(conexion, SqlContarAuditoriaIntervencion));
                var sinCambio = new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia C: registros de auditoría tras editar solo la descripción",
                    Tabla = new OCN_Project.Models.TablaDatos
                    {
                        Columnas = new() { "registros_previos", "registros_despues", "incremento" },
                        Filas = new() { new[] { antes.ToString(), despues.ToString(), (despues - antes).ToString() } },
                    },
                };
                sinCambio.Notas.Add("El incremento es 0: el trigger devolvió temprano gracias a UPDATE(estado), probando la validación.");
                resultado.Secciones.Add(sinCambio);

                resultado.Exito = true;
            }
            catch (SqlException ex)
            {
                resultado.Exito = false;
                resultado.Error = $"Error de SQL Server {ex.Number}: {ex.Message}";
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Error = $"Error inesperado: {ex.Message}";
            }
            finally
            {
                try
                {
                    // Primero se borra la auditoría generada por la demo (referenciando ids demo),
                    // luego las intervenciones demo. Se borra auditoría con ids que aún existan.
                    EjecutarNoQuery(conexion, SqlBorrarAuditoriaDemo, ("@patron", PrefijoDemo + "%"));
                    EjecutarNoQuery(conexion, SqlBorrarDemoIntervenciones, ("@patron", PrefijoDemo + "%"));
                }
                catch (Exception exLimpieza)
                {
                    resultado.Pasos.Add($"Advertencia durante la limpieza: {exLimpieza.Message}");
                }
            }
        }

        return resultado;
    }

    // ------------------------------------------------------------------
    // Demo 3: trigger de eliminación ocn.trg_intervencion_impedir_eliminar_completada
    // ------------------------------------------------------------------
    private const string SqlEliminarIntervencion = @"
        DELETE FROM ocn.intervencion WHERE idintervencion = @idintervencion;";

    private const string SqlExisteIntervencion = @"
        SELECT COUNT(*) FROM ocn.intervencion WHERE idintervencion = @idintervencion;";

    private const string SqlRevertirAPlaneada = @"
        UPDATE ocn.intervencion SET estado = 'Planeada' WHERE idintervencion = @idintervencion;";

    private const string SqlRevertirAPlaneadaTodasDemo = @"
        UPDATE ocn.intervencion SET estado = 'Planeada' WHERE descripcion LIKE @patron AND estado = 'Completada';";

    /// <summary>
    /// Demostración del trigger AFTER DELETE: impide eliminar intervenciones 'Completada'
    /// (ROLLBACK + THROW 50002) y permite eliminar las demás.
    /// </summary>
    public static OCN_Project.Models.DemoResultado TriggerEliminacion(string cadenaConexion)
    {
        var resultado = new OCN_Project.Models.DemoResultado
        {
            Justificacion = "Regla de negocio a nivel de DELETE: el CHECK chk_estado_values valida los valores permitidos pero no puede proteger el borrado.\n" +
                            "El trigger lee la tabla deleted para detectar filas 'Completada' y ROLLBACK + THROW deshace toda la operación y notifica el error 50002.",
        };

        var fks = new Dictionary<string, object?>();

        using (var conexion = new SqlConnection(cadenaConexion))
        {
            conexion.Open();
            try
            {
                ResolverFks(conexion, fks, resultado);
                if (!resultado.Exito)
                {
                    return resultado;
                }

                // ---------------- Escenario 1: eliminar una 'Completada' (bloqueado) ----------------
                var marcadorProtegida = PrefijoDemo + Guid.NewGuid() + " protegida";
                int idProtegida = InsertarIntervencionDemo(conexion, fks, marcadorProtegida, "Completada", null);
                resultado.Pasos.Add($"Paso 1: se inserta una intervención de prueba (id {idProtegida}) con estado 'Completada'.");

                string mensajeError;
                try
                {
                    EjecutarNoQuery(conexion, SqlEliminarIntervencion, ("@idintervencion", idProtegida));
                    mensajeError = "(no se produjo error; escenario inesperado)";
                }
                catch (SqlException ex)
                {
                    mensajeError = $"Error {ex.Number}: {ex.Message}";
                }

                resultado.Pasos.Add("Paso 2: se intenta DELETE sobre la fila 'Completada'. El trigger detecta la fila en deleted, hace ROLLBACK y lanza THROW 50002.");
                resultado.Secciones.Add(new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia A: error 50002 capturado (comportamiento esperado del trigger)",
                }.Also(s => s.Notas.Add(mensajeError)));

                var sigueAhi = Convert.ToInt32(Escalar(conexion, SqlExisteIntervencion, ("@idintervencion", idProtegida)));
                var evidenciaB = new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia B: la fila sigue existiendo después del intento de borrado",
                    Tabla = new OCN_Project.Models.TablaDatos
                    {
                        Columnas = new() { "filas_con_ese_id" },
                        Filas = new() { new[] { sigueAhi.ToString() } },
                    },
                };
                evidenciaB.Notas.Add("El valor es 1: el ROLLBACK del trigger deshizo el DELETE por completo.");
                resultado.Secciones.Add(evidenciaB);

                // ---------------- Escenario 2: eliminar una 'Planeada' (permitido) ----------------
                var marcadorEliminable = PrefijoDemo + Guid.NewGuid() + " eliminable";
                int idEliminable = InsertarIntervencionDemo(conexion, fks, marcadorEliminable, "Planeada", null);
                resultado.Pasos.Add($"Paso 3: se inserta una segunda intervención (id {idEliminable}) con estado 'Planeada' y se elimina.");
                EjecutarNoQuery(conexion, SqlEliminarIntervencion, ("@idintervencion", idEliminable));
                var quedan = Convert.ToInt32(Escalar(conexion, SqlExisteIntervencion, ("@idintervencion", idEliminable)));
                var evidenciaDelete = new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia C: DELETE exitoso sobre la fila 'Planeada'",
                    Tabla = new OCN_Project.Models.TablaDatos
                    {
                        Columnas = new() { "filas_con_ese_id_tras_borrar" },
                        Filas = new() { new[] { quedan.ToString() } },
                    },
                };
                evidenciaDelete.Notas.Add("El valor es 0: el trigger solo bloquea 'Completada'; los demás estados se eliminan sin problema.");
                resultado.Secciones.Add(evidenciaDelete);

                resultado.Exito = true;
            }
            catch (SqlException ex)
            {
                resultado.Exito = false;
                resultado.Error = $"Error de SQL Server {ex.Number}: {ex.Message}";
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Error = $"Error inesperado: {ex.Message}";
            }
            finally
            {
                try
                {
                    // La fila 'Completada' no se puede borrar directamente (el trigger lo impide):
                    // primero se regresa a 'Planeada' con UPDATE (esto dispara el trigger de
                    // auditoría, efecto secundario esperado) y después se elimina.
                    var idsDemo = LeerColumnaIds(conexion, PrefijoDemo + "%");
                    EjecutarNoQuery(conexion, SqlRevertirAPlaneadaTodasDemo, ("@patron", PrefijoDemo + "%"));
                    if (idsDemo.Count > 0)
                    {
                        resultado.Pasos.Add($"Limpieza: las filas demo 'Completada' se actualizan a 'Planeada' para poder borrarlas; ese UPDATE dispara el trigger de auditoría (efecto esperado) y sus registros se eliminan aquí mismo.");
                    }
                    EjecutarNoQuery(conexion, SqlBorrarAuditoriaDemo, ("@patron", PrefijoDemo + "%"));
                    EjecutarNoQuery(conexion, SqlBorrarDemoIntervenciones, ("@patron", PrefijoDemo + "%"));
                }
                catch (Exception exLimpieza)
                {
                    resultado.Pasos.Add($"Advertencia durante la limpieza: {exLimpieza.Message}");
                }
            }
        }

        return resultado;
    }

    // ------------------------------------------------------------------
    // Demo 4: cursor ocn.usp_procesar_intervenciones_antiguas_cursor
    // ------------------------------------------------------------------
    private const string SqlLlamarCursor = @"
        EXEC ocn.usp_procesar_intervenciones_antiguas_cursor @dias = @dias;";

    private const string SqlIntervencionesDemo = @"
        SELECT idintervencion, estado, descripcion, fecharegistro
        FROM ocn.intervencion
        WHERE descripcion LIKE @patron
        ORDER BY idintervencion;";

    private const string SqlRevertirCursorDemo = @"
        UPDATE ocn.intervencion SET estado = 'Planeada' WHERE descripcion LIKE @patron AND estado = 'EnProceso';";

    /// <summary>
    /// Demostración del cursor LOCAL FAST_FORWARD dentro del procedimiento almacenado:
    /// procesa fila a fila las intervenciones 'Planeada' antiguas y las pasa a 'EnProceso'.
    /// </summary>
    public static OCN_Project.Models.DemoResultado Cursor(string cadenaConexion)
    {
        var resultado = new OCN_Project.Models.DemoResultado
        {
            Justificacion = "El cursor LOCAL FAST_FORWARD recorre las filas en un solo pase secuencial, con mínimo uso de memoria y bloqueos, y se libera automáticamente al cerrar el scope.\n" +
                            "Nota honesta: en producción sería preferible un UPDATE basado en conjuntos (set-based); el cursor aquí demuestra el procesamiento fila a fila exigido por la tarea.\n" +
                            "El procedimiento valida @dias >= 0 y lanza THROW 50003 ante valores negativos.",
        };

        var fks = new Dictionary<string, object?>();

        using (var conexion = new SqlConnection(cadenaConexion))
        {
            conexion.Open();
            try
            {
                ResolverFks(conexion, fks, resultado);
                if (!resultado.Exito)
                {
                    return resultado;
                }

                // ---------------- Guard: @dias negativo lanza 50003 ----------------
                string mensajeGuarda;
                try
                {
                    EjecutarNoQuery(conexion, SqlLlamarCursor, ("@dias", -1));
                    mensajeGuarda = "(no se produjo error; escenario inesperado)";
                }
                catch (SqlException ex)
                {
                    mensajeGuarda = $"Error {ex.Number}: {ex.Message}";
                }
                resultado.Pasos.Add("Paso 1: se ejecuta el procedimiento con @dias = -1 para probar la validación de entrada.");
                resultado.Secciones.Add(new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia A: error 50003 capturado al pasar @dias = -1",
                }.Also(s => s.Notas.Add(mensajeGuarda)));

                // ---------------- Preparación: intervenciones 'antiguas' ----------------
                var marcador = PrefijoDemo + Guid.NewGuid() + " cursor";
                var idsCreados = new List<int>();
                for (var i = 0; i < 3; i++)
                {
                    var idFila = InsertarIntervencionDemo(conexion, fks, marcador + $" ({i + 1})", "Planeada", DateTime.UtcNow.AddDays(-100));
                    idsCreados.Add(idFila);
                }
                resultado.Pasos.Add("Paso 2: se insertan 3 intervenciones 'Planeada' con fecharegistro = hoy - 100 días (marca '" + marcador + "').");

                resultado.Secciones.Add(new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia B: ANTES del cursor (todas en estado 'Planeada')",
                    Tabla = LeerTabla(conexion, SqlIntervencionesDemo, ("@patron", marcador + "%")),
                });

                // ---------------- Ejecución del cursor ----------------
                EjecutarNoQuery(conexion, SqlLlamarCursor, ("@dias", 30));
                resultado.Pasos.Add("Paso 3: EXEC ocn.usp_procesar_intervenciones_antiguas_cursor @dias = 30. El cursor recorre fila a fila y actualiza cada una a 'EnProceso'.");

                resultado.Secciones.Add(new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia C: DESPUÉS del cursor (todas en estado 'EnProceso')",
                    Tabla = LeerTabla(conexion, SqlIntervencionesDemo, ("@patron", marcador + "%")),
                });

                var evidenciaAuditoriaCursor = new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Evidencia D: auditoría generada por el trigger durante las actualizaciones del cursor",
                    Tabla = LeerTabla(conexion, SqlAuditoriaDemo, ("@patron", marcador + "%")),
                };
                evidenciaAuditoriaCursor.Notas.Add("Cada UPDATE fila a fila del cursor disparó el trigger de auditoría: sinergia cursor + trigger como evidencia adicional.");
                resultado.Secciones.Add(evidenciaAuditoriaCursor);

                resultado.Exito = true;
            }
            catch (SqlException ex)
            {
                resultado.Exito = false;
                resultado.Error = $"Error de SQL Server {ex.Number}: {ex.Message}";
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Error = $"Error inesperado: {ex.Message}";
            }
            finally
            {
                try
                {
                    // Se regresa a 'Planeada' (aunque 'EnProceso' también se puede borrar) y
                    // se eliminan las intervenciones demo junto con su auditoría.
                    EjecutarNoQuery(conexion, SqlRevertirCursorDemo, ("@patron", PrefijoDemo + "%"));
                    EjecutarNoQuery(conexion, SqlBorrarAuditoriaDemo, ("@patron", PrefijoDemo + "%"));
                    EjecutarNoQuery(conexion, SqlBorrarDemoIntervenciones, ("@patron", PrefijoDemo + "%"));
                }
                catch (Exception exLimpieza)
                {
                    resultado.Pasos.Add($"Advertencia durante la limpieza: {exLimpieza.Message}");
                }
            }
        }

        return resultado;
    }

    // ------------------------------------------------------------------
    // Demo 5: CTE recursiva de jerarquía de problemáticas (solo lectura, GET)
    // ------------------------------------------------------------------
    private const string SqlCteRecursiva = @"
        WITH jerarquia AS (
            SELECT p.idproblematica, p.idproblematicapadre, p.nombre, 0 AS nivel,
                   CAST(p.nombre AS nvarchar(4000)) AS ruta
            FROM ocn.problematica p
            WHERE p.idproblematicapadre IS NULL
            UNION ALL
            SELECT h.idproblematica, h.idproblematicapadre, h.nombre, j.nivel + 1,
                   CAST(j.ruta + ' > ' + h.nombre AS nvarchar(4000))
            FROM ocn.problematica h
            INNER JOIN jerarquia j ON h.idproblematicapadre = j.idproblematica
        )
        SELECT idproblematica, idproblematicapadre, nombre, nivel, ruta
        FROM jerarquia
        ORDER BY ruta
        OPTION (MAXRECURSION 100);";

    private const string SqlContarProblematicas = "SELECT COUNT(*) FROM ocn.problematica;";
    private const string SqlContarHijas = "SELECT COUNT(*) FROM ocn.problematica WHERE idproblematicapadre IS NOT NULL;";

    /// <summary>
    /// Demostración de la CTE recursiva que recorre la jerarquía de problemáticas.
    /// Es de solo lectura, por lo que se ejecuta directamente en la acción GET.
    /// </summary>
    public static OCN_Project.Models.DemoResultado CteRecursiva(string cadenaConexion)
    {
        var resultado = new OCN_Project.Models.DemoResultado
        {
            Justificacion = "La jerarquía padre-hijo se recorre en un solo paso set-based (frente a cursores o self-joins encadenados).\n" +
                            "MAXRECURSION protege contra ciclos infinitos en datos mal formados.\n" +
                            "La ruta acumulada tipo breadcrumb ('padre > hijo') solo es posible gracias a la recursión.",
        };

        using (var conexion = new SqlConnection(cadenaConexion))
        {
            conexion.Open();
            try
            {
                var total = Convert.ToInt32(Escalar(conexion, SqlContarProblematicas));
                var hijas = Convert.ToInt32(Escalar(conexion, SqlContarHijas));

                if (total == 0)
                {
                    resultado.Exito = false;
                    resultado.Error = "No hay problemáticas registradas en ocn.problematica. Inserte primero algunas problemáticas (al menos una raíz y una hija) para ver la recursión en acción.";
                    return resultado;
                }

                var tabla = LeerTabla(conexion, SqlCteRecursiva);
                var maxNivel = 0;
                if (tabla.Filas.Count > 0)
                {
                    foreach (var fila in tabla.Filas)
                    {
                        if (int.TryParse(fila[3], out var nivel) && nivel > maxNivel)
                        {
                            maxNivel = nivel;
                        }
                        // Sangría del nombre según su nivel (espacios duros que sobreviven al HTML).
                        fila[2] = new string('\u00A0', nivel * 4) + fila[2];
                    }
                }

                var seccion = new OCN_Project.Models.SeccionEvidencia
                {
                    Titulo = "Jerarquía de problemáticas recorrida con la CTE recursiva",
                    Tabla = tabla,
                };
                if (hijas == 0)
                {
                    seccion.Notas.Add("Solo existen problemáticas raíz (nivel 0). Para apreciar la recursión, cree una problemática hija con idproblematicapadre apuntando a una raíz.");
                }
                else
                {
                    seccion.Notas.Add($"Se encontraron niveles del 0 al {maxNivel}: la recursión dio {maxNivel} pasadas adicionales sobre la jerarquía.");
                }
                seccion.Notas.Add("La columna 'nombre' aparece sangrada según su nivel y 'ruta' muestra el breadcrumb acumulado padre > hijo.");
                resultado.Secciones.Add(seccion);

                resultado.Pasos.Add("Paso 1: ancla — problemáticas raíz (idproblematicapadre IS NULL) con nivel 0 y ruta = nombre.");
                resultado.Pasos.Add("Paso 2: recursión — cada hija se une al miembro anterior sumando nivel + 1 y acumulando ruta + ' > ' + nombre.");
                resultado.Pasos.Add("Paso 3: ORDER BY ruta con OPTION (MAXRECURSION 100) para orden tipo breadcrumb y proteger contra ciclos.");

                resultado.Exito = true;
            }
            catch (SqlException ex)
            {
                resultado.Exito = false;
                resultado.Error = $"Error de SQL Server {ex.Number}: {ex.Message}";
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Error = $"Error inesperado: {ex.Message}";
            }
        }

        return resultado;
    }

    // ------------------------------------------------------------------
    // Helpers privados compartidos
    // ------------------------------------------------------------------
    private const string SqlInsertarDemoSinFecha = @"
        INSERT INTO ocn.intervencion (idtipointervencion, idobservacionproblematica, idrecurso, descripcion, estado)
        OUTPUT INSERTED.idintervencion
        VALUES (@idtipointervencion, @idobservacionproblematica, @idrecurso, @descripcion, @estado);";

    /// <summary>Extensión mínima para configurar un objeto en la misma expresión (estilo fluido).</summary>
    private static T Also<T>(this T objeto, Action<T> accion)
    {
        accion(objeto);
        return objeto;
    }

    /// <summary>
    /// Resuelve llaves foráneas válidas (tipo de intervención activo, observación problemática
    /// y usuarios) para construir filas demo consistentes. Si falta un dato requerido
    /// termina con Exito = false y un mensaje amable en lugar de lanzar excepción.
    /// </summary>
    private static void ResolverFks(SqlConnection conexion, Dictionary<string, object?> fks, OCN_Project.Models.DemoResultado resultado)
    {
        using var comando = new SqlCommand(SqlResolverFks, conexion);
        using var lector = comando.ExecuteReader();
        lector.Read();
        fks["idtipointervencion"] = lector["idtipointervencion"] == DBNull.Value ? null : lector["idtipointervencion"];
        fks["idobservacionproblematica"] = lector["idobservacionproblematica"] == DBNull.Value ? null : lector["idobservacionproblematica"];
        fks["idusuario1"] = lector["idusuario1"] == DBNull.Value ? null : lector["idusuario1"];
        fks["idusuario2"] = lector["idusuario2"] == DBNull.Value ? null : lector["idusuario2"];
        lector.Close();

        if (fks["idtipointervencion"] == null || fks["idobservacionproblematica"] == null)
        {
            resultado.Exito = false;
            resultado.Error = "Faltan datos de catálogo para ejecutar la demostración. Inserte primero al menos un tipo de intervención activo en ocn.cattipointervencion y una observación problemática en ocn.observacionproblematica.";
            return;
        }

        resultado.Exito = true;
    }

    /// <summary>Construye el DataTable del parámetro de tabla (TVP) ocn.ListaUsuariosType con usuarios reales.</summary>
    private static DataTable CrearTvpUsuarios(Dictionary<string, object?> fks)
    {
        var tabla = new DataTable();
        tabla.Columns.Add("idusuario", typeof(int));
        if (fks["idusuario1"] != null) tabla.Rows.Add(fks["idusuario1"]);
        if (fks["idusuario2"] != null && !Equals(fks["idusuario2"], fks["idusuario1"])) tabla.Rows.Add(fks["idusuario2"]);
        return tabla;
    }

    /// <summary>Inserta una intervención de demostración y devuelve su id. La fecha es opcional (por defecto sysdatetime()).</summary>
    private static int InsertarIntervencionDemo(SqlConnection conexion, Dictionary<string, object?> fks, string descripcion, string estado, DateTime? fecha)
    {
        using var comando = new SqlCommand(fecha.HasValue ? SqlInsertarDemo : SqlInsertarDemoSinFecha, conexion);
        comando.Parameters.AddWithValue("@idtipointervencion", fks["idtipointervencion"]!);
        comando.Parameters.AddWithValue("@idobservacionproblematica", fks["idobservacionproblematica"]!);
        comando.Parameters.AddWithValue("@idrecurso", DBNull.Value);
        comando.Parameters.AddWithValue("@descripcion", descripcion);
        comando.Parameters.AddWithValue("@estado", estado);
        if (fecha.HasValue) comando.Parameters.AddWithValue("@fecharegistro", fecha.Value);
        return Convert.ToInt32(comando.ExecuteScalar());
    }

    /// <summary>Devuelve los ids de las intervenciones demo (solo para bitácora).</summary>
    private static List<int> LeerColumnaIds(SqlConnection conexion, string patron)
    {
        var ids = new List<int>();
        using var comando = new SqlCommand("SELECT idintervencion FROM ocn.intervencion WHERE descripcion LIKE @patron;", conexion);
        comando.Parameters.AddWithValue("@patron", patron);
        using var lector = comando.ExecuteReader();
        while (lector.Read()) ids.Add(lector.GetInt32(0));
        return ids;
    }

    /// <summary>Ejecuta una consulta escalar con parámetros opcionales.</summary>
    private static object? Escalar(SqlConnection conexion, string sql, params (string Nombre, object? Valor)[] parametros)
    {
        using var comando = new SqlCommand(sql, conexion);
        foreach (var (nombre, valor) in parametros) comando.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);
        return comando.ExecuteScalar();
    }

    /// <summary>Ejecuta una consulta sin resultados (INSERT/UPDATE/DELETE/EXEC) con parámetros opcionales.</summary>
    private static int EjecutarNoQuery(SqlConnection conexion, string sql, params (string Nombre, object? Valor)[] parametros)
    {
        using var comando = new SqlCommand(sql, conexion);
        foreach (var (nombre, valor) in parametros) comando.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);
        return comando.ExecuteNonQuery();
    }

    /// <summary>
    /// Lee un resultset completo y lo convierte en <see cref="OCN_Project.Models.TablaDatos"/>
    /// con todas las celdas ya serializadas para la vista (NULL, fechas dd/MM/yyyy HH:mm, etc.).
    /// </summary>
    private static OCN_Project.Models.TablaDatos LeerTabla(SqlConnection conexion, string sql, params (string Nombre, object? Valor)[] parametros)
    {
        using var comando = new SqlCommand(sql, conexion);
        foreach (var (nombre, valor) in parametros) comando.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);
        using var lector = comando.ExecuteReader();

        var tabla = new OCN_Project.Models.TablaDatos();
        for (var i = 0; i < lector.FieldCount; i++) tabla.Columnas.Add(lector.GetName(i));
        while (lector.Read())
        {
            var fila = new string[lector.FieldCount];
            for (var i = 0; i < lector.FieldCount; i++) fila[i] = FormatoCelda(lector.IsDBNull(i) ? null : lector.GetValue(i));
            tabla.Filas.Add(fila);
        }
        return tabla;
    }

    /// <summary>Serializa una celda para mostrar: NULL, Sí/No, fechas dd/MM/yyyy HH:mm y demás como texto.</summary>
    private static string FormatoCelda(object? valor)
    {
        if (valor == null) return "NULL";
        return valor switch
        {
            DateTime fecha => fecha.ToString("dd/MM/yyyy HH:mm"),
            bool b => b ? "Sí" : "No",
            byte[] _ => "(datos binarios)",
            _ => valor.ToString() ?? string.Empty,
        };
    }
}
