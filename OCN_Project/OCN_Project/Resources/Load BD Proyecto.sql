
use proyecto_ocn
go
set nocount on;
go

/* ----------------------------------------------------------------------------
   utilidades temporales de generación (numeros y sesgo por categoría)
   ---------------------------------------------------------------------------- */
if object_id('tempdb..#numeros') is not null drop table #numeros;
create table #numeros (n int primary key);
go

;with seq as (
    select 1 as n
    union all
    select n + 1 from seq where n < 500
)
insert into #numeros (n)
select n from seq
option (maxrecursion 500);
go

if object_id('tempdb..#sesgo_categoria') is not null drop table #sesgo_categoria;
create table #sesgo_categoria (nombre varchar(100) primary key, sesgo int);
go

insert into #sesgo_categoria (nombre, sesgo) values
('adicciones', 1),
('salud mental', 1),
('violencia', 1),
('empleo y economía familiar', -1),
('vivienda', -1),
('acceso digital', -1),
('educación', 0), ('alimentación', 0), ('salud física', 0), ('relaciones familiares', 0),
('niñez y adolescencia', 0), ('adultos mayores', 0), ('servicios básicos', 0), ('movilidad', 0),
('seguridad', 0), ('inclusión y discapacidad', 0), ('medio ambiente', 0),
('participación comunitaria', 0), ('acceso a programas y servicios', 0);
go

/* ============================================================================
   1. seguridad: roles y usuarios
   ============================================================================ */

insert into ocn.rol (nombrerol, descripcion) values
('administrador', 'control total del sistema y catálogos'),
('coordinador', 'supervisión de levantamientos y validación de información'),
('capturista', 'registro de levantamientos y observaciones'),
('analista', 'consulta de información agregada y generación de reportes'),
('consulta', 'acceso de solo lectura a reportes e indicadores');
go

insert into ocn.usuario (nombreusuario, nombrecompleto, correoinstitucional) values
('mgarcia', 'maría garcía torres', 'mgarcia@um.edu.mx'),
('jrodriguez', 'jorge rodríguez lomelí', 'jrodriguez@um.edu.mx'),
('lcastillo', 'lucía castillo peña', 'lcastillo@um.edu.mx'),
('rhernandez', 'ricardo hernández solís', 'rhernandez@um.edu.mx'),
('pnajera', 'paola nájera cantú', 'pnajera@um.edu.mx'),
('eflores', 'eduardo flores villarreal', 'eflores@um.edu.mx');
go

insert into ocn.usuariorol (idusuario, idrol)
select u.idusuario, r.idrol
from ocn.usuario u
join ocn.rol r on
       (u.nombreusuario = 'mgarcia'    and r.nombrerol = 'coordinador')
    or (u.nombreusuario = 'jrodriguez' and r.nombrerol = 'administrador')
    or (u.nombreusuario = 'lcastillo'  and r.nombrerol = 'capturista')
    or (u.nombreusuario = 'rhernandez' and r.nombrerol = 'capturista')
    or (u.nombreusuario = 'pnajera'    and r.nombrerol = 'analista')
    or (u.nombreusuario = 'eflores'    and r.nombrerol = 'consulta');
go

/* ============================================================================
   2. territorio: divisiones y zonas
   ============================================================================ */

insert into ocn.cattipozona (nombre) values ('urbana'), ('periurbana'), ('rural');
go

insert into ocn.divisionterritorial (nombre, tipo) values
('colonia fundadores', 'colonia'),
('sector la popular', 'colonia'),
('ejido san rafael', 'comunidad rural'),
('colonia las palmas', 'colonia');
go

insert into ocn.zona (codigozona, nombre, iddivision, idtipozona, descripciongeneral, referenciageograficaaprox)
values
('z-001', 'fundadores norte',
    (select iddivision from ocn.divisionterritorial where nombre = 'colonia fundadores'),
    (select idtipozona from ocn.cattipozona where nombre = 'urbana'),
    'sector consolidado con alta densidad habitacional', 'zona centro-norte del municipio'),
('z-002', 'fundadores sur',
    (select iddivision from ocn.divisionterritorial where nombre = 'colonia fundadores'),
    (select idtipozona from ocn.cattipozona where nombre = 'urbana'),
    'sector con comercio mixto y vivienda', 'zona centro-sur del municipio'),
('z-003', 'la popular oriente',
    (select iddivision from ocn.divisionterritorial where nombre = 'sector la popular'),
    (select idtipozona from ocn.cattipozona where nombre = 'periurbana'),
    'sector en expansión con servicios parciales', 'periferia oriente'),
('z-004', 'la popular poniente',
    (select iddivision from ocn.divisionterritorial where nombre = 'sector la popular'),
    (select idtipozona from ocn.cattipozona where nombre = 'periurbana'),
    'sector con infraestructura vial limitada', 'periferia poniente'),
('z-005', 'ejido san rafael centro',
    (select iddivision from ocn.divisionterritorial where nombre = 'ejido san rafael'),
    (select idtipozona from ocn.cattipozona where nombre = 'rural'),
    'comunidad rural dispersa', 'zona rural sur del municipio'),
('z-006', 'las palmas',
    (select iddivision from ocn.divisionterritorial where nombre = 'colonia las palmas'),
    (select idtipozona from ocn.cattipozona where nombre = 'urbana'),
    'sector de reciente urbanización', 'zona oriente del municipio');
go

/* ============================================================================
   3. periodos, métodos e instrumentos
   ============================================================================ */

insert into ocn.periodo (codigoperiodo, nombre, fechainicio, fechafin) values
('2025-b', 'agosto - diciembre 2025', '2025-08-01', '2025-12-15'),
('2026-a', 'enero - junio 2026', '2026-01-15', '2026-06-30');
go

insert into ocn.catmetodo (nombre) values
('encuesta'), ('entrevista'), ('grupo focal'), ('observación de campo'), ('recorrido comunitario');
go

insert into ocn.instrumento (nombrefamilia, version, idmetodo, estado, fechaversion) values
('cuestionario de necesidades comunitarias', '1.0',
    (select idmetodo from ocn.catmetodo where nombre = 'encuesta'), 'vigente', '2025-07-15'),
('guía de entrevista semiestructurada', '1.0',
    (select idmetodo from ocn.catmetodo where nombre = 'entrevista'), 'vigente', '2025-07-15'),
('guía de grupo focal comunitario', '1.0',
    (select idmetodo from ocn.catmetodo where nombre = 'grupo focal'), 'vigente', '2025-07-20'),
('formato de observación de campo', '1.0',
    (select idmetodo from ocn.catmetodo where nombre = 'observación de campo'), 'vigente', '2025-07-20'),
('formato de recorrido comunitario', '1.0',
    (select idmetodo from ocn.catmetodo where nombre = 'recorrido comunitario'), 'vigente', '2025-07-22');
go

-- versión 2 del cuestionario de encuesta (rn-31: no sobrescribe la versión anterior)
insert into ocn.instrumento (nombrefamilia, version, idmetodo, estado, idinstrumentoanterior, fechaversion)
values
('cuestionario de necesidades comunitarias', '2.0',
    (select idmetodo from ocn.catmetodo where nombre = 'encuesta'), 'vigente',
    (select idinstrumento from ocn.instrumento
        where nombrefamilia = 'cuestionario de necesidades comunitarias' and version = '1.0'),
    '2026-01-05');
go

/* ============================================================================
   4. fuentes de información y organizaciones
   ============================================================================ */

insert into ocn.cattipofuente (nombre) values
('encuesta'), ('entrevista'), ('líder comunitario'), ('escuela'), ('centro de salud'),
('organización comunitaria'), ('institución religiosa'), ('observación directa'), ('grupo focal');
go

insert into ocn.cattipoorganizacion (nombre) values
('gubernamental'), ('educativa'), ('sanitaria'), ('religiosa'), ('asociación civil'),
('comunitaria'), ('privada'), ('otra');
go

insert into ocn.organizacion (nombre, idtipoorganizacion) values
('dif municipal montemorelos', (select idtipoorganizacion from ocn.cattipoorganizacion where nombre = 'gubernamental')),
('secundaria técnica no. 12', (select idtipoorganizacion from ocn.cattipoorganizacion where nombre = 'educativa')),
('centro de salud fundadores', (select idtipoorganizacion from ocn.cattipoorganizacion where nombre = 'sanitaria')),
('parroquia san rafael', (select idtipoorganizacion from ocn.cattipoorganizacion where nombre = 'religiosa')),
('asociación vecinal la popular', (select idtipoorganizacion from ocn.cattipoorganizacion where nombre = 'asociación civil')),
('comité comunitario las palmas', (select idtipoorganizacion from ocn.cattipoorganizacion where nombre = 'comunitaria')),
('universidad de montemorelos - fitec', (select idtipoorganizacion from ocn.cattipoorganizacion where nombre = 'educativa')),
('cruz roja montemorelos', (select idtipoorganizacion from ocn.cattipoorganizacion where nombre = 'privada')),
('club deportivo fundadores', (select idtipoorganizacion from ocn.cattipoorganizacion where nombre = 'comunitaria')),
('dif estatal nuevo león - módulo montemorelos', (select idtipoorganizacion from ocn.cattipoorganizacion where nombre = 'gubernamental'));
go

insert into ocn.fuente (idtipofuente, idorganizacion, descripcion) values
((select idtipofuente from ocn.cattipofuente where nombre = 'encuesta'), null,
    'levantamiento de encuestas domiciliarias agregadas por sector'),
((select idtipofuente from ocn.cattipofuente where nombre = 'entrevista'), null,
    'entrevistas estructuradas a informantes clave'),
((select idtipofuente from ocn.cattipofuente where nombre = 'líder comunitario'),
    (select idorganizacion from ocn.organizacion where nombre = 'comité comunitario las palmas'),
    'liderazgo comunitario reconocido en la zona'),
((select idtipofuente from ocn.cattipofuente where nombre = 'escuela'),
    (select idorganizacion from ocn.organizacion where nombre = 'secundaria técnica no. 12'),
    'reporte institucional escolar'),
((select idtipofuente from ocn.cattipofuente where nombre = 'centro de salud'),
    (select idorganizacion from ocn.organizacion where nombre = 'centro de salud fundadores'),
    'reporte de unidad de salud'),
((select idtipofuente from ocn.cattipofuente where nombre = 'organización comunitaria'),
    (select idorganizacion from ocn.organizacion where nombre = 'asociación vecinal la popular'),
    'información recabada por asociación vecinal'),
((select idtipofuente from ocn.cattipofuente where nombre = 'institución religiosa'),
    (select idorganizacion from ocn.organizacion where nombre = 'parroquia san rafael'),
    'observación pastoral comunitaria'),
((select idtipofuente from ocn.cattipofuente where nombre = 'observación directa'), null,
    'observación directa del equipo de campo'),
((select idtipofuente from ocn.cattipofuente where nombre = 'grupo focal'), null,
    'sesión de grupo focal con participantes comunitarios'),
((select idtipofuente from ocn.cattipofuente where nombre = 'organización comunitaria'),
    (select idorganizacion from ocn.organizacion where nombre = 'club deportivo fundadores'),
    'reporte de actividades deportivas comunitarias');
go

/* ============================================================================
   5. grupos poblacionales
   ============================================================================ */

insert into ocn.catgrupopoblacional (nombre, rangoedad) values
('primera infancia', '0-5'),
('niñez', '6-11'),
('adolescencia', '12-15'),
('jóvenes', '16-25'),
('adultos', '26-59'),
('adultos mayores', '60 o más'),
('mujeres', null),
('hombres', null),
('personas con discapacidad', null),
('estudiantes', null),
('personas fuera del sistema escolar', null),
('población trabajadora', null),
('personas desempleadas', null),
('cuidadores', null),
('población migrante', null);
go

/* ============================================================================
   6. categorías y problemáticas (jerárquico)
   ============================================================================ */

insert into ocn.categoriaproblematica (nombre) values
('educación'), ('empleo y economía familiar'), ('alimentación'), ('salud física'), ('salud mental'),
('adicciones'), ('relaciones familiares'), ('violencia'), ('niñez y adolescencia'), ('adultos mayores'),
('vivienda'), ('servicios básicos'), ('movilidad'), ('seguridad'), ('inclusión y discapacidad'),
('acceso digital'), ('medio ambiente'), ('participación comunitaria'), ('acceso a programas y servicios');
go

insert into ocn.problematica (idcategoria, nombre, descripcion) values
((select idcategoria from ocn.categoriaproblematica where nombre = 'educación'), 'desvinculación escolar', 'alumnos que reducen o abandonan su participación en actividades escolares'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'educación'), 'rezago académico', 'dificultad sostenida para alcanzar los aprendizajes esperados'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'empleo y economía familiar'), 'precariedad laboral', 'empleo informal o inestable que no cubre necesidades básicas'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'empleo y economía familiar'), 'desempleo prolongado', 'ausencia de empleo por periodos extendidos'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'alimentación'), 'inseguridad alimentaria', 'acceso limitado o irregular a alimentos suficientes'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'salud física'), 'acceso limitado a servicios de salud', 'dificultad para recibir atención médica oportuna'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'salud mental'), 'malestar emocional', 'manifestaciones sostenidas de tristeza, ansiedad o desmotivación'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'adicciones'), 'consumo de sustancias', 'uso de alcohol, tabaco u otras sustancias en el entorno comunitario'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'relaciones familiares'), 'conflicto familiar', 'tensiones sostenidas dentro del núcleo familiar'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'violencia'), 'violencia en el entorno comunitario', 'manifestaciones de violencia percibidas en la zona'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'niñez y adolescencia'), 'falta de espacios recreativos seguros', 'ausencia de espacios adecuados para actividades infantiles y juveniles'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'adultos mayores'), 'aislamiento de adultos mayores', 'reducción de redes de apoyo y participación social'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'vivienda'), 'condiciones inadecuadas de vivienda', 'viviendas con carencias estructurales o de servicios'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'servicios básicos'), 'servicios básicos insuficientes', 'cobertura limitada de agua, drenaje o electricidad'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'movilidad'), 'transporte público insuficiente', 'dificultad para trasladarse dentro y fuera de la zona'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'seguridad'), 'percepción de inseguridad', 'percepción comunitaria de riesgo en espacios públicos'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'inclusión y discapacidad'), 'barreras de accesibilidad', 'obstáculos físicos o sociales para personas con discapacidad'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'acceso digital'), 'brecha de acceso digital', 'limitaciones de conectividad o dispositivos para acceder a servicios digitales'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'medio ambiente'), 'manejo inadecuado de residuos', 'disposición irregular de basura en la zona'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'participación comunitaria'), 'baja participación comunitaria', 'escasa participación en actividades u organizaciones comunitarias'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'acceso a programas y servicios'), 'desconocimiento de programas sociales', 'falta de información sobre programas y apoyos disponibles');
go

-- subproblemáticas (jerarquía de segundo nivel)
insert into ocn.problematica (idcategoria, idproblematicapadre, nombre, descripcion) values
((select idcategoria from ocn.categoriaproblematica where nombre = 'educación'),
    (select idproblematica from ocn.problematica where nombre = 'desvinculación escolar'),
    'inasistencia frecuente', 'ausencias reiteradas sin justificación'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'salud mental'),
    (select idproblematica from ocn.problematica where nombre = 'malestar emocional'),
    'ansiedad percibida', 'manifestaciones de ansiedad reportadas por informantes'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'adicciones'),
    (select idproblematica from ocn.problematica where nombre = 'consumo de sustancias'),
    'consumo de alcohol', 'consumo de bebidas alcohólicas en el entorno'),
((select idcategoria from ocn.categoriaproblematica where nombre = 'adicciones'),
    (select idproblematica from ocn.problematica where nombre = 'consumo de sustancias'),
    'consumo de vapeadores', 'uso de cigarrillos electrónicos entre adolescentes y jóvenes');
go

/* ============================================================================
   7. escalas: magnitud, gravedad, frecuencia, tendencia
   ============================================================================ */

insert into ocn.catmagnitud (nombre, orden) values
('no determinada', 0), ('casos aislados', 1), ('pocos casos', 2),
('presencia moderada', 3), ('presencia frecuente', 4), ('presencia generalizada', 5);
go

insert into ocn.catgravedad (nombre, orden) values
('baja', 1), ('moderada', 2), ('importante', 3), ('grave', 4), ('crítica', 5);
go

insert into ocn.catfrecuencia (nombre, orden) values
('no determinada', 0), ('aislada', 1), ('ocasional', 2),
('mensual', 3), ('semanal', 4), ('frecuente', 5), ('permanente', 6);
go

insert into ocn.cattendencia (nombre) values
('nueva'), ('disminuyendo'), ('estable'), ('aumentando'), ('recurrente'), ('indeterminada');
go

/* ============================================================================
   8. necesidades, factores de riesgo y factores protectores
   ============================================================================ */

insert into ocn.necesidad (nombre, descripcion) values
('apoyo académico', 'reforzamiento escolar para estudiantes en rezago'),
('transporte escolar', 'traslado seguro hacia centros educativos'),
('apoyo económico', 'apoyo monetario o en especie para familias'),
('orientación familiar', 'acompañamiento para el manejo de conflictos familiares'),
('acceso digital', 'conectividad y equipo para actividades escolares y laborales'),
('orientación psicológica', 'atención emocional individual o grupal'),
('terapia especializada', 'atención terapéutica para casos de mayor complejidad'),
('grupos de apoyo', 'espacios de acompañamiento entre pares'),
('atención médica oportuna', 'acceso a consulta y seguimiento médico'),
('programas de prevención de adicciones', 'acciones orientadas a reducir el consumo de sustancias'),
('espacios recreativos seguros', 'infraestructura para actividades deportivas y culturales'),
('programas para adultos mayores', 'actividades de integración y acompañamiento'),
('mejoramiento de vivienda', 'apoyo para reparación o ampliación de vivienda'),
('ampliación de servicios básicos', 'extensión de agua, drenaje o electricidad'),
('mejora del transporte público', 'incremento de rutas o frecuencia de transporte'),
('programas de seguridad comunitaria', 'acciones para reducir la percepción de inseguridad'),
('accesibilidad para personas con discapacidad', 'adecuaciones físicas y de servicios'),
('capacitación laboral', 'formación para la inserción o mejora del empleo'),
('bolsa de empleo comunitaria', 'vinculación laboral para población desempleada'),
('orientación sobre programas sociales', 'difusión de apoyos y trámites disponibles'),
('manejo adecuado de residuos', 'estrategias de recolección y disposición de basura'),
('promoción de la participación comunitaria', 'fomento a la organización vecinal'),
('acompañamiento a cuidadores', 'apoyo para quienes cuidan a personas dependientes'),
('atención a la primera infancia', 'programas de estimulación y cuidado temprano');
go

insert into ocn.catfactorriesgo (nombre) values
('desempleo'), ('precariedad económica'), ('conflicto familiar'), ('abandono escolar'),
('aislamiento social'), ('consumo de sustancias en el entorno'), ('falta de transporte'),
('falta de servicios'), ('ausencia de espacios recreativos'), ('hacinamiento'),
('falta de redes de apoyo'), ('inseguridad en el entorno');
go

insert into ocn.catfactorprotector (nombre) values
('apoyo familiar'), ('escuela'), ('organizaciones comunitarias'), ('iglesias'),
('asociaciones civiles'), ('liderazgo comunitario'), ('espacios deportivos'),
('programas juveniles'), ('instituciones de salud'), ('participación vecinal'), ('voluntariado');
go

/* ============================================================================
   9. recursos comunitarios y servicios
   ============================================================================ */

insert into ocn.cattiporecurso (nombre) values
('salud'), ('educación'), ('deporte'), ('asistencia social'), ('orientación psicológica'),
('empleo'), ('alimentación'), ('organización comunitaria');
go

insert into ocn.catestadorecurso (nombre) values
('activo'), ('temporal'), ('suspendido'), ('cerrado'), ('desconocido');
go

insert into ocn.catcostorecurso (nombre) values
('gratuito'), ('subsidiado'), ('con costo');
go

insert into ocn.recurso (codigorecurso, nombre, idtiporecurso, idorganizacionresponsable, idestadorecurso, idcostorecurso, horariogeneral, capacidadaproximada, conocimientocomunidad)
values
('rec-001', 'centro de salud fundadores',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'salud'),
    (select idorganizacion from ocn.organizacion where nombre = 'centro de salud fundadores'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'gratuito'),
    'lunes a viernes 8:00-15:00', 60, 'alto'),
('rec-002', 'secundaria técnica no. 12',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'educación'),
    (select idorganizacion from ocn.organizacion where nombre = 'secundaria técnica no. 12'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'gratuito'),
    'lunes a viernes 7:00-14:00', 400, 'alto'),
('rec-003', 'club deportivo fundadores',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'deporte'),
    (select idorganizacion from ocn.organizacion where nombre = 'club deportivo fundadores'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'subsidiado'),
    'martes a domingo 16:00-20:00', 80, 'moderado'),
('rec-004', 'dif municipal - módulo asistencia social',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'asistencia social'),
    (select idorganizacion from ocn.organizacion where nombre = 'dif municipal montemorelos'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'gratuito'),
    'lunes a viernes 9:00-16:00', 100, 'alto'),
('rec-005', 'módulo de orientación psicológica um-fitec',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'orientación psicológica'),
    (select idorganizacion from ocn.organizacion where nombre = 'universidad de montemorelos - fitec'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'subsidiado'),
    'lunes a viernes 10:00-13:00', 30, 'bajo'),
('rec-006', 'bolsa de trabajo dif estatal',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'empleo'),
    (select idorganizacion from ocn.organizacion where nombre = 'dif estatal nuevo león - módulo montemorelos'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'gratuito'),
    'lunes a viernes 9:00-14:00', 50, 'moderado'),
('rec-007', 'comedor comunitario la popular',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'alimentación'),
    (select idorganizacion from ocn.organizacion where nombre = 'asociación vecinal la popular'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'subsidiado'),
    'lunes a sábado 12:00-15:00', 120, 'alto'),
('rec-008', 'parroquia san rafael - grupo de apoyo',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'organización comunitaria'),
    (select idorganizacion from ocn.organizacion where nombre = 'parroquia san rafael'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'gratuito'),
    'domingos 10:00-12:00', 40, 'alto'),
('rec-009', 'cruz roja montemorelos - atención básica',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'salud'),
    (select idorganizacion from ocn.organizacion where nombre = 'cruz roja montemorelos'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'con costo'),
    '24 horas urgencias', 20, 'alto'),
('rec-010', 'comité comunitario las palmas - actividades juveniles',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'organización comunitaria'),
    (select idorganizacion from ocn.organizacion where nombre = 'comité comunitario las palmas'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'temporal'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'gratuito'),
    'sábados 16:00-18:00', 35, 'moderado'),
('rec-011', 'dif municipal - programa adultos mayores',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'asistencia social'),
    (select idorganizacion from ocn.organizacion where nombre = 'dif municipal montemorelos'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'gratuito'),
    'martes y jueves 10:00-12:00', 45, 'moderado'),
('rec-012', 'centro comunitario digital um-fitec',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'educación'),
    (select idorganizacion from ocn.organizacion where nombre = 'universidad de montemorelos - fitec'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'gratuito'),
    'lunes a viernes 15:00-18:00', 25, 'bajo'),
('rec-013', 'centro de salud ejido san rafael',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'salud'),
    (select idorganizacion from ocn.organizacion where nombre = 'dif estatal nuevo león - módulo montemorelos'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'desconocido'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'gratuito'),
    'lunes, miércoles y viernes 8:00-12:00', 15, 'bajo'),
('rec-014', 'asociación vecinal la popular - orientación social',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'asistencia social'),
    (select idorganizacion from ocn.organizacion where nombre = 'asociación vecinal la popular'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'gratuito'),
    'miércoles 9:00-13:00', 30, 'moderado'),
('rec-015', 'club deportivo fundadores - escuela de futbol infantil',
    (select idtiporecurso from ocn.cattiporecurso where nombre = 'deporte'),
    (select idorganizacion from ocn.organizacion where nombre = 'club deportivo fundadores'),
    (select idestadorecurso from ocn.catestadorecurso where nombre = 'activo'),
    (select idcostorecurso from ocn.catcostorecurso where nombre = 'subsidiado'),
    'sábados 9:00-11:00', 40, 'alto');
go

insert into ocn.catservicio (nombre) values
('consulta médica general'), ('atención de urgencias básicas'), ('orientación psicológica individual'),
('grupos de apoyo emocional'), ('regularización académica'), ('actividades deportivas'),
('comedor comunitario'), ('entrega de despensas'), ('bolsa de trabajo'), ('capacitación laboral'),
('acompañamiento a adultos mayores'), ('acceso a equipo de cómputo e internet'),
('orientación sobre programas sociales'), ('actividades recreativas juveniles'),
('prevención de adicciones'), ('acompañamiento familiar'), ('atención a primera infancia'),
('recolección comunitaria de residuos');
go

insert into ocn.cattipocobertura (nombre) values
('zona'), ('colonia'), ('sector'), ('municipio'), ('región');
go

insert into ocn.catbarrera (nombre) values
('distancia'), ('transporte'), ('costo'), ('horario'), ('desconocimiento'), ('cupo insuficiente'),
('requisitos administrativos'), ('barrera física'), ('estigma'), ('barrera lingüística');
go

insert into ocn.cattipoevidencia (nombre) values
('encuesta'), ('entrevista'), ('observación'), ('documento institucional'), ('reporte escolar'), ('grupo focal');
go

insert into ocn.cattipointervencion (nombre) values
('apoyo académico'), ('transporte escolar'), ('apoyo económico'), ('orientación familiar'),
('acceso digital'), ('atención psicológica'), ('atención médica'), ('programas recreativos'),
('capacitación laboral'), ('acompañamiento comunitario');
go

/* ============================================================================
   10. levantamientos (2 por combinación zona x periodo = 24)
   ============================================================================ */

;with zp as (
    select z.idzona, z.codigozona, p.idperiodo, p.codigoperiodo, p.fechainicio, p.fechafin
    from ocn.zona z
    cross join ocn.periodo p
), nums as (
    select 1 as k union all select 2
)
insert into ocn.levantamiento (codigolevantamiento, idzona, idperiodo, idmetodo, fechalevantamiento, numeroparticipantesaprox, observacionesmetodologicas)
select
    'lev-' + zp.codigoperiodo + '-' + right('000' + cast(zp.idzona as varchar(3)), 3) + '-' + cast(nums.k as varchar(1)),
    zp.idzona,
    zp.idperiodo,
    m.idmetodo,
    dateadd(day, (zp.idzona * 3 + nums.k * 5) % datediff(day, zp.fechainicio, zp.fechafin), zp.fechainicio),
    20 + ((zp.idzona * 7 + nums.k * 3) % 40),
    'levantamiento realizado conforme al protocolo institucional del ocn.'
from zp
cross join nums
cross join (select idmetodo, row_number() over (order by idmetodo) as rn from ocn.catmetodo) m
where m.rn = ((zp.idzona + nums.k) % 5) + 1;
go

-- instrumento asociado a cada levantamiento (usa la v2 de encuesta a partir del periodo 2026-a)
insert into ocn.levantamientoinstrumento (idlevantamiento, idinstrumento)
select
    l.idlevantamiento,
    case
        when cm.nombre = 'encuesta' and p.codigoperiodo = '2026-a' then
            (select idinstrumento from ocn.instrumento where nombrefamilia = 'cuestionario de necesidades comunitarias' and version = '2.0')
        when cm.nombre = 'encuesta' then
            (select idinstrumento from ocn.instrumento where nombrefamilia = 'cuestionario de necesidades comunitarias' and version = '1.0')
        else
            (select top 1 idinstrumento from ocn.instrumento where idmetodo = l.idmetodo)
    end
from ocn.levantamiento l
join ocn.periodo p on p.idperiodo = l.idperiodo
join ocn.catmetodo cm on cm.idmetodo = l.idmetodo;
go

-- responsables: un capturista rotativo + la coordinadora en todos los levantamientos
;with capturistas as (
    select u.idusuario, row_number() over (order by u.idusuario) as rn
    from ocn.usuario u
    join ocn.usuariorol ur on ur.idusuario = u.idusuario
    join ocn.rol r on r.idrol = ur.idrol and r.nombrerol = 'capturista'
)
insert into ocn.levantamientoresponsable (idlevantamiento, idusuario, rolenlevantamiento)
select l.idlevantamiento, c.idusuario, 'responsable de campo'
from ocn.levantamiento l
join capturistas c on c.rn = ((l.idlevantamiento % 2) + 1);
go

insert into ocn.levantamientoresponsable (idlevantamiento, idusuario, rolenlevantamiento)
select l.idlevantamiento, u.idusuario, 'supervisión'
from ocn.levantamiento l
cross join (select idusuario from ocn.usuario where nombreusuario = 'mgarcia') u;
go

-- fuentes utilizadas en cada levantamiento (observación directa + una fuente rotativa)
insert into ocn.levantamientofuente (idlevantamiento, idfuente)
select l.idlevantamiento, f.idfuente
from ocn.levantamiento l
cross join ocn.fuente f
where f.descripcion = 'observación directa del equipo de campo';
go

;with fuentes_num as (
    select idfuente, row_number() over (order by idfuente) as rn from ocn.fuente
)
insert into ocn.levantamientofuente (idlevantamiento, idfuente)
select l.idlevantamiento, fn.idfuente
from ocn.levantamiento l
join fuentes_num fn on fn.rn = ((l.idlevantamiento % 10) + 1)
where not exists (
    select 1 from ocn.levantamientofuente lf
    where lf.idlevantamiento = l.idlevantamiento and lf.idfuente = fn.idfuente
);
go

/* ============================================================================
   11. observaciones (entre 3 y 5 por levantamiento)
   ============================================================================ */

insert into ocn.observacion (idlevantamiento, notacontexto)
select l.idlevantamiento,
    case (n.n % 6)
        when 0 then 'se reporta dificultad recurrente para que adolescentes accedan a actividades recreativas.'
        when 1 then 'se identifican expresiones de malestar emocional entre jóvenes del sector.'
        when 2 then 'se observan carencias en el mantenimiento de espacios públicos.'
        when 3 then 'se reportan dificultades económicas que afectan la alimentación familiar.'
        when 4 then 'se menciona un incremento en el consumo de sustancias entre adolescentes.'
        else 'se identifican necesidades relacionadas con el acceso a servicios básicos.'
    end
from ocn.levantamiento l
cross join #numeros n
where n.n <= 3 + (l.idlevantamiento % 3);
go

-- fuentes de cada observación (hereda hasta 2 de las fuentes de su levantamiento)
;with of_candidates as (
    select o.idobservacion, lf.idfuente, row_number() over (partition by o.idobservacion order by lf.idfuente) as rn
    from ocn.observacion o
    join ocn.levantamientofuente lf on lf.idlevantamiento = o.idlevantamiento
)
insert into ocn.observacionfuente (idobservacion, idfuente)
select idobservacion, idfuente from of_candidates where rn <= 2;
go

-- grupos poblacionales de cada observación (1 a 2 grupos)
;with gp_num as (
    select idgrupopoblacional, row_number() over (order by idgrupopoblacional) as rn from ocn.catgrupopoblacional
), gp_count as (
    select count(*) as total from ocn.catgrupopoblacional
)
insert into ocn.observaciongrupopoblacional (idobservacion, idgrupopoblacional)
select o.idobservacion, gp.idgrupopoblacional
from ocn.observacion o
cross join gp_count c
join gp_num gp on gp.rn = ((o.idobservacion % c.total) + 1)
union
select o.idobservacion, gp2.idgrupopoblacional
from ocn.observacion o
cross join gp_count c
join gp_num gp2 on gp2.rn = (((o.idobservacion + 5) % c.total) + 1);
go

/* ============================================================================
   12. observacionproblematica (hecho central): 1 a 2 problemáticas por
       observación, con magnitud, gravedad y frecuencia derivadas por
       fórmula y sesgadas por periodo según la categoría (ver #sesgo_categoria)
   ============================================================================ */

;with prob_num as (
    select idproblematica, idcategoria, row_number() over (order by idproblematica) as rn from ocn.problematica
), prob_count as (
    select count(*) as total from ocn.problematica
), obs_prob_pairs as (
    select o.idobservacion, o.idlevantamiento, pn.idproblematica, pn.idcategoria
    from ocn.observacion o
    cross join prob_count c
    join prob_num pn on pn.rn = ((o.idobservacion % c.total) + 1)
    union
    select o.idobservacion, o.idlevantamiento, pn2.idproblematica, pn2.idcategoria
    from ocn.observacion o
    cross join prob_count c
    join prob_num pn2 on pn2.rn = (((o.idobservacion * 3 + 7) % c.total) + 1)
)
insert into ocn.observacionproblematica (idobservacion, idproblematica, idmagnitud, idgravedad, idfrecuencia, nivelconfianza)
select
    opp.idobservacion,
    opp.idproblematica,
    mg.idmagnitud,
    gr.idgravedad,
    fr.idfrecuencia,
    cast(40 + ((opp.idobservacion * 13 + opp.idproblematica * 7) % 55) as decimal(5,2))
from obs_prob_pairs opp
join ocn.levantamiento l on l.idlevantamiento = opp.idlevantamiento
join ocn.periodo per on per.idperiodo = l.idperiodo
left join ocn.categoriaproblematica cp on cp.idcategoria = opp.idcategoria
left join #sesgo_categoria sc on sc.nombre = cp.nombre
join ocn.catmagnitud mg on mg.orden =
    ((opp.idobservacion + opp.idproblematica
        + (case when per.codigoperiodo = '2026-a' then isnull(sc.sesgo, 0) else 0 end) - 1) % 5 + 5) % 5 + 1
join ocn.catgravedad gr on gr.orden =
    ((opp.idobservacion * 2 + opp.idproblematica
        + (case when per.codigoperiodo = '2026-a' then isnull(sc.sesgo, 0) else 0 end) - 1) % 5 + 5) % 5 + 1
join ocn.catfrecuencia fr on fr.orden =
    ((opp.idobservacion + opp.idproblematica * 3
        + (case when per.codigoperiodo = '2026-a' then isnull(sc.sesgo, 0) else 0 end) - 1) % 6 + 6) % 6 + 1;
go

/* ============================================================================
   13. evidencia (una por cada observacionproblematica)
   ============================================================================ */

;with ev_base as (
    select op.idobservacionproblematica, op.idobservacion,
           row_number() over (order by op.idobservacionproblematica) as rn
    from ocn.observacionproblematica op
), tipo_num as (
    select idtipoevidencia, row_number() over (order by idtipoevidencia) as rn from ocn.cattipoevidencia
), tipo_count as (
    select count(*) as total from ocn.cattipoevidencia
)
insert into ocn.evidencia (idobservacionproblematica, idtipoevidencia, idfuente, numeromenciones, descripcionbreve)
select
    eb.idobservacionproblematica,
    tn.idtipoevidencia,
    (select top 1 of_.idfuente from ocn.observacionfuente of_ where of_.idobservacion = eb.idobservacion order by of_.idfuente),
    1 + ((eb.rn * 3) % 12),
    'evidencia registrada durante el levantamiento correspondiente, sin datos que permitan identificar personas.'
from ev_base eb
cross join tipo_count tc
join tipo_num tn on tn.rn = ((eb.rn % tc.total) + 1);
go

/* ============================================================================
   14. relaciones de catálogo (problemática-necesidad, factores, servicios,
       rutas de intervención)
   ============================================================================ */

-- problemática x necesidad (2 por problemática)
;with p_num as (select idproblematica, row_number() over (order by idproblematica) as rn from ocn.problematica),
 n_num as (select idnecesidad, row_number() over (order by idnecesidad) as rn from ocn.necesidad),
 n_count as (select count(*) as total from ocn.necesidad)
insert into ocn.problematicanecesidad (idproblematica, idnecesidad)
select p.idproblematica, n.idnecesidad
from p_num p cross join n_count c join n_num n on n.rn = ((p.rn % c.total) + 1)
union
select p.idproblematica, n2.idnecesidad
from p_num p cross join n_count c join n_num n2 on n2.rn = (((p.rn + 4) % c.total) + 1);
go

-- problemática x factor de riesgo (2 por problemática)
;with p_num as (select idproblematica, row_number() over (order by idproblematica) as rn from ocn.problematica),
 fr_num as (select idfactorriesgo, row_number() over (order by idfactorriesgo) as rn from ocn.catfactorriesgo),
 fr_count as (select count(*) as total from ocn.catfactorriesgo)
insert into ocn.problematicafactorriesgo (idproblematica, idfactorriesgo)
select p.idproblematica, f.idfactorriesgo
from p_num p cross join fr_count c join fr_num f on f.rn = ((p.rn % c.total) + 1)
union
select p.idproblematica, f2.idfactorriesgo
from p_num p cross join fr_count c join fr_num f2 on f2.rn = (((p.rn + 3) % c.total) + 1);
go

-- problemática x factor protector (2 por problemática)
;with p_num as (select idproblematica, row_number() over (order by idproblematica) as rn from ocn.problematica),
 fp_num as (select idfactorprotector, row_number() over (order by idfactorprotector) as rn from ocn.catfactorprotector),
 fp_count as (select count(*) as total from ocn.catfactorprotector)
insert into ocn.problematicafactorprotector (idproblematica, idfactorprotector)
select p.idproblematica, fp.idfactorprotector
from p_num p cross join fp_count c join fp_num fp on fp.rn = ((p.rn % c.total) + 1)
union
select p.idproblematica, fp2.idfactorprotector
from p_num p cross join fp_count c join fp_num fp2 on fp2.rn = (((p.rn + 2) % c.total) + 1);
go

-- factor protector x recurso existente (rn-91)
;with fp_num as (select idfactorprotector, row_number() over (order by idfactorprotector) as rn from ocn.catfactorprotector),
 r_num as (select idrecurso, row_number() over (order by idrecurso) as rn from ocn.recurso),
 r_count as (select count(*) as total from ocn.recurso)
insert into ocn.factorprotectorrecurso (idfactorprotector, idrecurso)
select fp.idfactorprotector, r.idrecurso
from fp_num fp cross join r_count c join r_num r on r.rn = ((fp.rn % c.total) + 1);
go

-- recurso x servicio (2 por recurso)
;with r_num as (select idrecurso, row_number() over (order by idrecurso) as rn from ocn.recurso),
 s_num as (select idservicio, row_number() over (order by idservicio) as rn from ocn.catservicio),
 s_count as (select count(*) as total from ocn.catservicio)
insert into ocn.recursoservicio (idrecurso, idservicio)
select r.idrecurso, s.idservicio
from r_num r cross join s_count c join s_num s on s.rn = ((r.rn % c.total) + 1)
union
select r.idrecurso, s2.idservicio
from r_num r cross join s_count c join s_num s2 on s2.rn = (((r.rn + 3) % c.total) + 1);
go

-- servicio x necesidad (2 por servicio)
;with s_num as (select idservicio, row_number() over (order by idservicio) as rn from ocn.catservicio),
 n_num as (select idnecesidad, row_number() over (order by idnecesidad) as rn from ocn.necesidad),
 n_count as (select count(*) as total from ocn.necesidad)
insert into ocn.servicionecesidad (idservicio, idnecesidad)
select s.idservicio, n.idnecesidad
from s_num s cross join n_count c join n_num n on n.rn = ((s.rn % c.total) + 1)
union
select s.idservicio, n2.idnecesidad
from s_num s cross join n_count c join n_num n2 on n2.rn = (((s.rn + 5) % c.total) + 1);
go

-- necesidad x tipo de intervención
;with n_num as (select idnecesidad, row_number() over (order by idnecesidad) as rn from ocn.necesidad),
 ti_num as (select idtipointervencion, row_number() over (order by idtipointervencion) as rn from ocn.cattipointervencion),
 ti_count as (select count(*) as total from ocn.cattipointervencion)
insert into ocn.necesidadtipointervencion (idnecesidad, idtipointervencion)
select n.idnecesidad, ti.idtipointervencion
from n_num n cross join ti_count c join ti_num ti on ti.rn = ((n.rn % c.total) + 1);
go

-- tipo de intervención x servicio
;with ti_num as (select idtipointervencion, row_number() over (order by idtipointervencion) as rn from ocn.cattipointervencion),
 s_num as (select idservicio, row_number() over (order by idservicio) as rn from ocn.catservicio),
 s_count as (select count(*) as total from ocn.catservicio)
insert into ocn.tipointervencionservicio (idtipointervencion, idservicio)
select ti.idtipointervencion, s.idservicio
from ti_num ti cross join s_count c join s_num s on s.rn = ((ti.rn % c.total) + 1)
union
select ti.idtipointervencion, s2.idservicio
from ti_num ti cross join s_count c join s_num s2 on s2.rn = (((ti.rn + 4) % c.total) + 1);
go

/* ============================================================================
   15. cobertura, barreras y accesibilidad (recurso + zona)
   ============================================================================ */

;with r_num as (select idrecurso, row_number() over (order by idrecurso) as rn from ocn.recurso),
 z_num as (select idzona, row_number() over (order by idzona) as rn from ocn.zona),
 z_count as (select count(*) as total from ocn.zona)
insert into ocn.recursozonacobertura (idrecurso, idzona, idtipocobertura, fechainicio, fechafin)
select r.idrecurso, z.idzona,
    (select idtipocobertura from ocn.cattipocobertura where nombre = 'zona'),
    '2025-07-01', null
from r_num r cross join z_count c join z_num z on z.rn = ((r.rn % c.total) + 1)
union
select r.idrecurso, z2.idzona,
    (select idtipocobertura from ocn.cattipocobertura where nombre = 'municipio'),
    '2025-07-01', null
from r_num r cross join z_count c join z_num z2 on z2.rn = (((r.rn + 2) % c.total) + 1);
go

;with rz as (
    select distinct idrecurso, idzona from ocn.recursozonacobertura
), rz_num as (
    select idrecurso, idzona, row_number() over (order by idrecurso, idzona) as rn from rz
), b_num as (
    select idbarrera, row_number() over (order by idbarrera) as rn from ocn.catbarrera
), b_count as (
    select count(*) as total from ocn.catbarrera
)
insert into ocn.recursozonabarrera (idrecurso, idzona, idbarrera, fecharegistro, notas)
select rz_num.idrecurso, rz_num.idzona, b.idbarrera, '2026-02-01',
       'barrera identificada durante el levantamiento comunitario.'
from rz_num
cross join b_count c
join b_num b on b.rn = ((rz_num.rn % c.total) + 1)
where rz_num.rn % 2 = 0;
go

;with rz as (
    select distinct idrecurso, idzona from ocn.recursozonacobertura
), rz_num as (
    select idrecurso, idzona, row_number() over (order by idrecurso, idzona) as rn from rz
)
insert into ocn.recursozonaaccesibilidad (idrecurso, idzona, nivelaccesibilidadpercibida, fechaevaluacion)
select idrecurso, idzona,
    case (rn % 4)
        when 0 then 'alta'
        when 1 then 'media'
        when 2 then 'baja'
        else 'muy baja'
    end,
    '2026-02-01'
from rz_num;
go

/* ============================================================================
   16. modelos de priorización (ponderaciones separadas de los datos)
   ============================================================================ */

insert into ocn.modelopriorizacion (nombre, descripcion) values
('modelo estándar', 'pondera de manera equilibrada gravedad, magnitud, frecuencia, evidencia, tendencia y brecha'),
('modelo orientado a gravedad', 'da mayor peso a la gravedad y la vulnerabilidad del grupo poblacional');
go

insert into ocn.modeloponderacion (idmodelo, variable, peso)
select idmodelo, 'magnitud', 0.15 from ocn.modelopriorizacion where nombre = 'modelo estándar'
union all select idmodelo, 'gravedad', 0.20 from ocn.modelopriorizacion where nombre = 'modelo estándar'
union all select idmodelo, 'frecuencia', 0.15 from ocn.modelopriorizacion where nombre = 'modelo estándar'
union all select idmodelo, 'vulnerabilidad', 0.15 from ocn.modelopriorizacion where nombre = 'modelo estándar'
union all select idmodelo, 'evidencia', 0.15 from ocn.modelopriorizacion where nombre = 'modelo estándar'
union all select idmodelo, 'tendencia', 0.10 from ocn.modelopriorizacion where nombre = 'modelo estándar'
union all select idmodelo, 'brecha', 0.10 from ocn.modelopriorizacion where nombre = 'modelo estándar'
union all select idmodelo, 'magnitud', 0.10 from ocn.modelopriorizacion where nombre = 'modelo orientado a gravedad'
union all select idmodelo, 'gravedad', 0.30 from ocn.modelopriorizacion where nombre = 'modelo orientado a gravedad'
union all select idmodelo, 'frecuencia', 0.10 from ocn.modelopriorizacion where nombre = 'modelo orientado a gravedad'
union all select idmodelo, 'vulnerabilidad', 0.20 from ocn.modelopriorizacion where nombre = 'modelo orientado a gravedad'
union all select idmodelo, 'evidencia', 0.10 from ocn.modelopriorizacion where nombre = 'modelo orientado a gravedad'
union all select idmodelo, 'tendencia', 0.10 from ocn.modelopriorizacion where nombre = 'modelo orientado a gravedad'
union all select idmodelo, 'brecha', 0.10 from ocn.modelopriorizacion where nombre = 'modelo orientado a gravedad';
go

/* ============================================================================
   17. tendenciaproblematica: calculada comparando 2025-b vs 2026-a
       por zona + problemática (rn-79 a rn-81, rn-205, rn-209)
   ============================================================================ */

;with agg as (
    select z.idzona, op.idproblematica, per.codigoperiodo,
           avg(cast(mg.orden as float)) as avgmag
    from ocn.observacionproblematica op
    join ocn.observacion o on o.idobservacion = op.idobservacion
    join ocn.levantamiento l on l.idlevantamiento = o.idlevantamiento
    join ocn.zona z on z.idzona = l.idzona
    join ocn.periodo per on per.idperiodo = l.idperiodo
    join ocn.catmagnitud mg on mg.idmagnitud = op.idmagnitud
    group by z.idzona, op.idproblematica, per.codigoperiodo
),
piv as (
    select idzona, idproblematica,
        max(case when codigoperiodo = '2025-b' then avgmag end) as avgmag_2025b,
        max(case when codigoperiodo = '2026-a' then avgmag end) as avgmag_2026a
    from agg
    group by idzona, idproblematica
)
insert into ocn.tendenciaproblematica (idzona, idproblematica, idperiodo, idtendencia)
select
    piv.idzona, piv.idproblematica,
    (select idperiodo from ocn.periodo where codigoperiodo = '2026-a'),
    ct.idtendencia
from piv
join ocn.cattendencia ct on ct.nombre =
    case
        when piv.avgmag_2025b is null and piv.avgmag_2026a is not null then 'nueva'
        when piv.avgmag_2026a > piv.avgmag_2025b then 'aumentando'
        when piv.avgmag_2026a < piv.avgmag_2025b then 'disminuyendo'
        else 'estable'
    end
where piv.avgmag_2026a is not null;
go

/* ============================================================================
   18. brechaatencion: necesidad + zona + periodo vs. recursos disponibles
       (rn-133 a rn-138)
   ============================================================================ */

;with necesidad_recursos as (
    select distinct n.idnecesidad, rs.idrecurso
    from ocn.necesidad n
    join ocn.servicionecesidad sn on sn.idnecesidad = n.idnecesidad
    join ocn.recursoservicio rs on rs.idservicio = sn.idservicio
),
zona_periodo as (
    select z.idzona, p.idperiodo
    from ocn.zona z cross join ocn.periodo p
),
cobertura as (
    select distinct rzc.idrecurso, rzc.idzona
    from ocn.recursozonacobertura rzc
    where rzc.fechafin is null
),
disponibilidad as (
    select zp.idzona, n.idnecesidad, zp.idperiodo,
           count(distinct c.idrecurso) as recursosidentificados
    from zona_periodo zp
    cross join ocn.necesidad n
    left join necesidad_recursos nr on nr.idnecesidad = n.idnecesidad
    left join cobertura c on c.idrecurso = nr.idrecurso and c.idzona = zp.idzona
    group by zp.idzona, n.idnecesidad, zp.idperiodo
)
insert into ocn.brechaatencion (idzona, idnecesidad, idperiodo, recursosidentificados, nivelbrecha)
select idzona, idnecesidad, idperiodo, recursosidentificados,
    case
        when recursosidentificados = 0 then 'sin cobertura'
        when recursosidentificados = 1 then 'cobertura parcial'
        else 'cobertura amplia'
    end
from disponibilidad;
go

/* ============================================================================
   19. prioridadcalculada: puntaje por zona + problemática + periodo,
       para cada modelo de ponderación (rn-126 a rn-132)
   ============================================================================ */

;with base_metrics as (
    select z.idzona, op.idproblematica, per.idperiodo, per.codigoperiodo,
           avg(cast(mg.orden as float)) / 5.0 as m_magnitud,
           avg(cast(gr.orden as float)) / 5.0 as m_gravedad,
           avg(cast(fr.orden as float)) / 6.0 as m_frecuencia,
           count(distinct ev.idevidencia) as n_evidencias,
           count(distinct ogp.idgrupopoblacional) as n_grupos
    from ocn.observacionproblematica op
    join ocn.observacion o on o.idobservacion = op.idobservacion
    join ocn.levantamiento l on l.idlevantamiento = o.idlevantamiento
    join ocn.zona z on z.idzona = l.idzona
    join ocn.periodo per on per.idperiodo = l.idperiodo
    join ocn.catmagnitud mg on mg.idmagnitud = op.idmagnitud
    join ocn.catgravedad gr on gr.idgravedad = op.idgravedad
    join ocn.catfrecuencia fr on fr.idfrecuencia = op.idfrecuencia
    left join ocn.evidencia ev on ev.idobservacionproblematica = op.idobservacionproblematica
    left join ocn.observaciongrupopoblacional ogp on ogp.idobservacion = o.idobservacion
    group by z.idzona, op.idproblematica, per.idperiodo, per.codigoperiodo
),
total_grupos as (
    select count(*) as total from ocn.catgrupopoblacional
),
tendencia_score as (
    select tp.idzona, tp.idproblematica, tp.idperiodo,
        case ct.nombre
            when 'aumentando' then 1.0
            when 'nueva' then 0.9
            when 'recurrente' then 0.8
            when 'estable' then 0.5
            when 'disminuyendo' then 0.2
            else 0.5
        end as tendenciascore
    from ocn.tendenciaproblematica tp
    join ocn.cattendencia ct on ct.idtendencia = tp.idtendencia
),
brecha_score as (
    select z.idzona, pn.idproblematica, ba.idperiodo,
        avg(case ba.nivelbrecha
                when 'sin cobertura' then 1.0
                when 'cobertura parcial' then 0.5
                else 0.1
            end) as brechascore
    from ocn.brechaatencion ba
    join ocn.zona z on z.idzona = ba.idzona
    join ocn.problematicanecesidad pn on pn.idnecesidad = ba.idnecesidad
    group by z.idzona, pn.idproblematica, ba.idperiodo
),
metrics as (
    select bm.idzona, bm.idproblematica, bm.idperiodo,
        bm.m_magnitud, bm.m_gravedad, bm.m_frecuencia,
        (cast(bm.n_grupos as float) / tg.total) as m_vulnerabilidad,
        (case when bm.n_evidencias >= 5 then 1.0 else cast(bm.n_evidencias as float) / 5.0 end) as m_evidencia,
        isnull(ts.tendenciascore, 0.5) as m_tendencia,
        isnull(bs.brechascore, 0.5) as m_brecha
    from base_metrics bm
    cross join total_grupos tg
    left join tendencia_score ts on ts.idzona = bm.idzona and ts.idproblematica = bm.idproblematica and ts.idperiodo = bm.idperiodo
    left join brecha_score bs on bs.idzona = bm.idzona and bs.idproblematica = bm.idproblematica and bs.idperiodo = bm.idperiodo
),
pesos as (
    select mp.idmodelo,
        max(case when mo.variable = 'magnitud' then mo.peso end) as w_magnitud,
        max(case when mo.variable = 'gravedad' then mo.peso end) as w_gravedad,
        max(case when mo.variable = 'frecuencia' then mo.peso end) as w_frecuencia,
        max(case when mo.variable = 'vulnerabilidad' then mo.peso end) as w_vulnerabilidad,
        max(case when mo.variable = 'evidencia' then mo.peso end) as w_evidencia,
        max(case when mo.variable = 'tendencia' then mo.peso end) as w_tendencia,
        max(case when mo.variable = 'brecha' then mo.peso end) as w_brecha
    from ocn.modelopriorizacion mp
    join ocn.modeloponderacion mo on mo.idmodelo = mp.idmodelo
    group by mp.idmodelo
)
insert into ocn.prioridadcalculada
    (idzona, idproblematica, idperiodo, idgrupopoblacional, idmodelo,
     valormagnitud, valorgravedad, valorfrecuencia, valorvulnerabilidad, valorevidencia, valortendencia, valorbrecha,
     puntajeresultante)
select
    m.idzona, m.idproblematica, m.idperiodo, null, pe.idmodelo,
    round(m.m_magnitud * 100, 2), round(m.m_gravedad * 100, 2), round(m.m_frecuencia * 100, 2),
    round(m.m_vulnerabilidad * 100, 2), round(m.m_evidencia * 100, 2), round(m.m_tendencia * 100, 2), round(m.m_brecha * 100, 2),
    round(100.0 * (
        pe.w_magnitud       * m.m_magnitud +
        pe.w_gravedad       * m.m_gravedad +
        pe.w_frecuencia     * m.m_frecuencia +
        pe.w_vulnerabilidad * m.m_vulnerabilidad +
        pe.w_evidencia      * m.m_evidencia +
        pe.w_tendencia      * m.m_tendencia +
        pe.w_brecha         * m.m_brecha
    ), 2)
from metrics m
cross join pesos pe;
go

/* ============================================================================
   20. limpieza de objetos temporales y resumen de volumetría
   ============================================================================ */

if object_id('tempdb..#numeros') is not null drop table #numeros;
if object_id('tempdb..#sesgo_categoria') is not null drop table #sesgo_categoria;
go

select 'zona' as tabla, count(*) as filas from ocn.zona
union all select 'periodo', count(*) from ocn.periodo
union all select 'levantamiento', count(*) from ocn.levantamiento
union all select 'observacion', count(*) from ocn.observacion
union all select 'observacionproblematica', count(*) from ocn.observacionproblematica
union all select 'evidencia', count(*) from ocn.evidencia
union all select 'recurso', count(*) from ocn.recurso
union all select 'necesidad', count(*) from ocn.necesidad
union all select 'recursozonacobertura', count(*) from ocn.recursozonacobertura
union all select 'recursozonabarrera', count(*) from ocn.recursozonabarrera
union all select 'tendenciaproblematica', count(*) from ocn.tendenciaproblematica
union all select 'brechaatencion', count(*) from ocn.brechaatencion
union all select 'prioridadcalculada', count(*) from ocn.prioridadcalculada
order by tabla;
go

/* ============================================================================
   fin del script de datos simulados
   ============================================================================ */
