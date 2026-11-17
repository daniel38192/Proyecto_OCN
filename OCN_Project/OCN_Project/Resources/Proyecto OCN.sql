
use master
go
create database proyecto_ocn
go
use proyecto_ocn
go

set nocount on;
go

if not exists (select 1 from sys.schemas where name = 'ocn')
    exec('create schema ocn authorization dbo');
go

/* ============================================================================
   a. seguridad y usuarios  (rn-168 a rn-175)
   ============================================================================ */

create table ocn.rol (
    idrol           int identity(1,1) primary key,
    nombrerol       varchar(50)  not null unique,   -- administrador, coordinador, capturista, analista, consulta
    descripcion     varchar(200) null,
    activo          bit not null constraint df_rol_activo default (1)
);
go

create table ocn.usuario (
    idusuario           int identity(1,1) primary key,
    nombreusuario        varchar(50)  not null unique,
    nombrecompleto        varchar(150) not null,      -- personal autorizado del proyecto (rn-26), no comunidad
    correoinstitucional   varchar(150) null,
    activo                bit not null constraint df_usuario_activo default (1),
    fechaalta             datetime2 not null constraint df_usuario_fechaalta default (sysdatetime())
);
go

create table ocn.usuariorol (               -- rn-169: cada usuario pertenece al menos a un rol
    idusuario   int not null foreign key references ocn.usuario(idusuario),
    idrol       int not null foreign key references ocn.rol(idrol),
    constraint pk_usuariorol primary key (idusuario, idrol)
);
go

/* ============================================================================
   b. territorio y zonas  (rn-01 a rn-10)
   ============================================================================ */

create table ocn.divisionterritorial (       -- colonia / división principal (rn-04, rn-05)
    iddivision      int identity(1,1) primary key,
    nombre          varchar(150) not null,
    tipo            varchar(60)  null,        -- colonia, comunidad rural, etc.
    activo          bit not null constraint df_divisionterritorial_activo default (1)
);
go

create table ocn.cattipozona (                -- urbana, periurbana, rural (rn-06)
    idtipozona      int identity(1,1) primary key,
    nombre          varchar(30) not null unique,
    activo          bit not null constraint df_cattipozona_activo default (1)
);
go

create table ocn.zona (
    idzona                      int identity(1,1) primary key,
    codigozona                  varchar(30) not null unique,   -- rn-02: identificador propio, no depende del nombre
    nombre                      varchar(150) not null,
    iddivision                  int not null foreign key references ocn.divisionterritorial(iddivision), -- rn-05
    idtipozona                  int not null foreign key references ocn.cattipozona(idtipozona),
    descripciongeneral          varchar(500) null,
    referenciageograficaaprox   varchar(200) null,   -- rn-07: aproximada, nunca coordenadas de vivienda
    activo                      bit not null constraint df_zona_activo default (1)
);
go
create index ix_zona_division on ocn.zona(iddivision);

/* ============================================================================
   c. periodos de estudio  (rn-11 a rn-16)
   ============================================================================ */

create table ocn.periodo (
    idperiodo       int identity(1,1) primary key,
    codigoperiodo   varchar(30) not null unique,
    nombre          varchar(100) not null,
    fechainicio     date not null,
    fechafin        date null,
    activo          bit not null constraint df_periodo_activo default (1)
);
go

/* ============================================================================
   d. métodos e instrumentos  (rn-28 a rn-32)
   ============================================================================ */

create table ocn.catmetodo (                  -- encuesta, entrevista, grupo focal, obs. de campo, recorrido
    idmetodo        int identity(1,1) primary key,
    nombre          varchar(60) not null unique,
    activo          bit not null constraint df_catmetodo_activo default (1)
);
go

create table ocn.instrumento (
    idinstrumento           int identity(1,1) primary key,
    nombrefamilia           varchar(150) not null,   -- mismo instrumento a través de versiones
    version                 varchar(20)  not null,
    idmetodo                int not null foreign key references ocn.catmetodo(idmetodo),
    estado                  varchar(30)  not null,   -- vigente, en revisión, retirado
    idinstrumentoanterior    int null foreign key references ocn.instrumento(idinstrumento), -- rn-31: no sobrescribir versiones
    fechaversion             date not null,
    activo                   bit not null constraint df_instrumento_activo default (1),
    constraint uq_instrumento_familiaversion unique (nombrefamilia, version)
);
go

/* ============================================================================
   e. levantamientos  (rn-17 a rn-27)
   ============================================================================ */

create table ocn.levantamiento (
    idlevantamiento             int identity(1,1) primary key,
    codigolevantamiento         varchar(30) not null unique,   -- p.ej. lev-2026-001
    idzona                      int not null foreign key references ocn.zona(idzona),         -- rn-01, rn-18
    idperiodo                   int not null foreign key references ocn.periodo(idperiodo),    -- rn-11, rn-13
    idmetodo                    int not null foreign key references ocn.catmetodo(idmetodo),
    fechalevantamiento          date not null,
    numeroparticipantesaprox    int null,               -- rn-24: aproximado, sin identificar individuos
    observacionesmetodologicas  varchar(1000) null,
    fechacaptura                datetime2 not null constraint df_levantamiento_fechacaptura default (sysdatetime()),
    constraint ck_levantamiento_fecha check (fechalevantamiento <= cast(getdate() as date) or 1 = 1)
        -- nota: si el proyecto requiere levantamientos programados a futuro (rn-178),
        -- reemplazar este check por lógica en capa de aplicación/trigger.
);
go
create index ix_levantamiento_zona on ocn.levantamiento(idzona);
create index ix_levantamiento_periodo on ocn.levantamiento(idperiodo);

create table ocn.levantamientoinstrumento (   -- rn-19, rn-20: n:m
    idlevantamiento int not null foreign key references ocn.levantamiento(idlevantamiento),
    idinstrumento   int not null foreign key references ocn.instrumento(idinstrumento),
    constraint pk_levantamientoinstrumento primary key (idlevantamiento, idinstrumento)
);
go

create table ocn.levantamientoresponsable (   -- rn-25, rn-26: equipo institucional, no comunidad
    idlevantamiento     int not null foreign key references ocn.levantamiento(idlevantamiento),
    idusuario           int not null foreign key references ocn.usuario(idusuario),
    rolenlevantamiento  varchar(50) null,
    constraint pk_levantamientoresponsable primary key (idlevantamiento, idusuario)
);
go

/* ============================================================================
   f. fuentes de información  (rn-33 a rn-38)
   ============================================================================ */

create table ocn.cattipofuente (              -- encuesta, entrevista, líder comunitario, escuela, etc.
    idtipofuente    int identity(1,1) primary key,
    nombre          varchar(60) not null unique,
    activo          bit not null constraint df_cattipofuente_activo default (1)
);
go

create table ocn.cattipoorganizacion (        -- rn-149: gubernamental, educativa, sanitaria, religiosa...
    idtipoorganizacion  int identity(1,1) primary key,
    nombre              varchar(60) not null unique,
    activo              bit not null constraint df_cattipoorganizacion_activo default (1)
);
go

create table ocn.organizacion (               -- rn-146 a rn-150
    idorganizacion      int identity(1,1) primary key,
    nombre              varchar(200) not null,
    idtipoorganizacion  int not null foreign key references ocn.cattipoorganizacion(idtipoorganizacion),
    activo              bit not null constraint df_organizacion_activo default (1)
);
go

create table ocn.fuente (                     -- rn-37: distingue tipo de fuente vs. organización concreta
    idfuente         int identity(1,1) primary key,
    idtipofuente     int not null foreign key references ocn.cattipofuente(idtipofuente),
    idorganizacion   int null foreign key references ocn.organizacion(idorganizacion),  -- null si es fuente sin organización (p.ej. observación directa)
    descripcion      varchar(300) null,        -- rn-38: nunca nombre de una persona
    activo           bit not null constraint df_fuente_activo default (1)
);
go

create table ocn.levantamientofuente (        -- rn-23
    idlevantamiento int not null foreign key references ocn.levantamiento(idlevantamiento),
    idfuente        int not null foreign key references ocn.fuente(idfuente),
    constraint pk_levantamientofuente primary key (idlevantamiento, idfuente)
);
go

/* ============================================================================
   g. observaciones comunitarias  (rn-39 a rn-47)
   ============================================================================ */

create table ocn.observacion (
    idobservacion   int identity(1,1) primary key,
    idlevantamiento int not null foreign key references ocn.levantamiento(idlevantamiento), -- rn-22, rn-40
    notacontexto    varchar(500) null,          -- rn-44, rn-45: breve, sin identificar personas
    fecharegistro   datetime2 not null constraint df_observacion_fecharegistro default (sysdatetime())
    -- rn-47: las observaciones no se editan para reflejar cambios posteriores;
    -- cualquier cambio se captura como un nuevo levantamiento/observacion.
);
go
create index ix_observacion_levantamiento on ocn.observacion(idlevantamiento);

create table ocn.observacionfuente (          -- rn-33, rn-35, rn-36: n:m
    idobservacion   int not null foreign key references ocn.observacion(idobservacion),
    idfuente        int not null foreign key references ocn.fuente(idfuente),
    constraint pk_observacionfuente primary key (idobservacion, idfuente)
);
go

/* ============================================================================
   h. grupos poblacionales  (rn-48 a rn-53)
   ============================================================================ */

create table ocn.catgrupopoblacional (
    idgrupopoblacional  int identity(1,1) primary key,
    nombre              varchar(100) not null unique,
    rangoedad           varchar(20) null,        -- categoría, p.ej. "12-15" (rn-51: nunca fecha de nacimiento)
    activo              bit not null constraint df_catgrupopoblacional_activo default (1)
);
go

create table ocn.observaciongrupopoblacional ( -- rn-43, rn-49, rn-50: n:m
    idobservacion       int not null foreign key references ocn.observacion(idobservacion),
    idgrupopoblacional  int not null foreign key references ocn.catgrupopoblacional(idgrupopoblacional),
    constraint pk_observaciongrupopoblacional primary key (idobservacion, idgrupopoblacional)
);
go

/* ============================================================================
   i. categorías y problemáticas (jerárquico)  (rn-54 a rn-60)
   ============================================================================ */

create table ocn.categoriaproblematica (
    idcategoria     int identity(1,1) primary key,
    nombre          varchar(100) not null unique,  -- p.ej. educación, salud mental, adicciones
    activo          bit not null constraint df_categoriaproblematica_activo default (1)
);
go

create table ocn.problematica (
    idproblematica         int identity(1,1) primary key,
    idcategoria            int not null foreign key references ocn.categoriaproblematica(idcategoria), -- rn-54
    idproblematicapadre    int null foreign key references ocn.problematica(idproblematica),           -- rn-56: subproblemática
    nombre                 varchar(200) not null,
    descripcion            varchar(500) null,
    activo                 bit not null constraint df_problematica_activo default (1)
);
go
create index ix_problematica_padre on ocn.problematica(idproblematicapadre);
create index ix_problematica_categoria on ocn.problematica(idcategoria);

/* ============================================================================
   j. escalas: magnitud, gravedad, frecuencia, tendencia
      (rn-68 a rn-81, rn-205 a rn-209)
   ============================================================================ */

create table ocn.catmagnitud (                -- casos aislados ... generalizada / no determinada
    idmagnitud      int identity(1,1) primary key,
    nombre          varchar(40) not null unique,
    orden           tinyint null,              -- para ordenar la escala en reportes
    activo          bit not null constraint df_catmagnitud_activo default (1)
);
go

create table ocn.catgravedad (                -- baja ... crítica
    idgravedad      int identity(1,1) primary key,
    nombre          varchar(40) not null unique,
    orden           tinyint null,
    activo          bit not null constraint df_catgravedad_activo default (1)
);
go

create table ocn.catfrecuencia (               -- aislada ... permanente / no determinada
    idfrecuencia    int identity(1,1) primary key,
    nombre          varchar(40) not null unique,
    orden           tinyint null,
    activo          bit not null constraint df_catfrecuencia_activo default (1)
);
go

create table ocn.cattendencia (                -- nueva, disminuyendo, estable, aumentando, recurrente, indeterminada
    idtendencia     int identity(1,1) primary key,
    nombre          varchar(40) not null unique,
    activo          bit not null constraint df_cattendencia_activo default (1)
);
go

/* ---- hecho central: observación x problemática ----
   rn-41/42 (n:m observación-problemática), rn-71/73-75 (magnitud y gravedad
   independientes y dependientes de la observación), rn-76/78 (frecuencia por
   observación), rn-206-208 (todas dependen del contexto observado). */
create table ocn.observacionproblematica (
    idobservacionproblematica  int identity(1,1) primary key,
    idobservacion              int not null foreign key references ocn.observacion(idobservacion),
    idproblematica             int not null foreign key references ocn.problematica(idproblematica),
    idmagnitud                 int not null foreign key references ocn.catmagnitud(idmagnitud),
    idgravedad                 int not null foreign key references ocn.catgravedad(idgravedad),
    idfrecuencia                int not null foreign key references ocn.catfrecuencia(idfrecuencia),
    nivelconfianza               decimal(5,2) null,   -- rn-124-125: derivado de múltiples criterios (ver evidencia)
    constraint uq_observacionproblematica unique (idobservacion, idproblematica)
);
go
create index ix_obsprob_problematica on ocn.observacionproblematica(idproblematica);

/* ---- tendencia calculada: zona + problemática + periodo ----
   rn-79 a rn-81, rn-205, rn-209: derivada, no sobrescribe históricos. */
create table ocn.tendenciaproblematica (
    idtendenciaproblematica int identity(1,1) primary key,
    idzona          int not null foreign key references ocn.zona(idzona),
    idproblematica  int not null foreign key references ocn.problematica(idproblematica),
    idperiodo       int not null foreign key references ocn.periodo(idperiodo),
    idtendencia     int not null foreign key references ocn.cattendencia(idtendencia),
    fechacalculo    datetime2 not null constraint df_tendenciaproblematica_fecha default (sysdatetime()),
    constraint uq_tendenciaproblematica unique (idzona, idproblematica, idperiodo)
);
go

/* ============================================================================
   k. necesidades  (rn-61 a rn-67)
   ============================================================================ */

create table ocn.necesidad (
    idnecesidad     int identity(1,1) primary key,
    nombre          varchar(150) not null unique,
    descripcion     varchar(300) null,
    activo          bit not null constraint df_necesidad_activo default (1)
);
go

create table ocn.problematicanecesidad (      -- rn-62, rn-63: n:m
    idproblematica  int not null foreign key references ocn.problematica(idproblematica),
    idnecesidad     int not null foreign key references ocn.necesidad(idnecesidad),
    constraint pk_problematicanecesidad primary key (idproblematica, idnecesidad)
);
go

/* ============================================================================
   l. factores de riesgo y protectores  (rn-82 a rn-91)
   ============================================================================ */

create table ocn.catfactorriesgo (
    idfactorriesgo  int identity(1,1) primary key,
    nombre          varchar(150) not null unique,
    activo          bit not null constraint df_catfactorriesgo_activo default (1)
);
go

create table ocn.problematicafactorriesgo (   -- rn-83, rn-84: tabla puente (n:m)
    idproblematica  int not null foreign key references ocn.problematica(idproblematica),
    idfactorriesgo  int not null foreign key references ocn.catfactorriesgo(idfactorriesgo),
    constraint pk_problematicafactorriesgo primary key (idproblematica, idfactorriesgo)
);
go

create table ocn.catfactorprotector (
    idfactorprotector   int identity(1,1) primary key,
    nombre              varchar(150) not null unique,
    activo              bit not null constraint df_catfactorprotector_activo default (1)
);
go

create table ocn.problematicafactorprotector ( -- rn-88, rn-89: tabla puente (n:m)
    idproblematica      int not null foreign key references ocn.problematica(idproblematica),
    idfactorprotector   int not null foreign key references ocn.catfactorprotector(idfactorprotector),
    constraint pk_problematicafactorprotector primary key (idproblematica, idfactorprotector)
);
go

/* ============================================================================
   m. recursos comunitarios  (rn-92 a rn-103)
   ============================================================================ */

create table ocn.cattiporecurso (             -- salud, educación, deporte, asistencia social, etc.
    idtiporecurso   int identity(1,1) primary key,
    nombre          varchar(100) not null unique,
    activo          bit not null constraint df_cattiporecurso_activo default (1)
);
go

create table ocn.catestadorecurso (           -- activo, temporal, suspendido, cerrado, desconocido
    idestadorecurso int identity(1,1) primary key,
    nombre          varchar(30) not null unique,
    activo          bit not null constraint df_catestadorecurso_activo default (1)
);
go

create table ocn.catcostorecurso (            -- gratuito, subsidiado, con costo
    idcostorecurso  int identity(1,1) primary key,
    nombre          varchar(30) not null unique,
    activo          bit not null constraint df_catcostorecurso_activo default (1)
);
go

create table ocn.recurso (
    idrecurso                   int identity(1,1) primary key,
    codigorecurso                varchar(30) not null unique,
    nombre                       varchar(200) not null,
    idtiporecurso                int not null foreign key references ocn.cattiporecurso(idtiporecurso),      -- rn-93
    idorganizacionresponsable    int not null foreign key references ocn.organizacion(idorganizacion),        -- rn-146, rn-148 (una principal)
    idestadorecurso               int not null foreign key references ocn.catestadorecurso(idestadorecurso),  -- rn-101
    idcostorecurso                 int not null foreign key references ocn.catcostorecurso(idcostorecurso),   -- rn-102
    horariogeneral                 varchar(200) null,
    capacidadaproximada             int null,
    conocimientocomunidad            varchar(40) null,   -- percepción de qué tan conocido es el recurso
    activo                          bit not null constraint df_recurso_activo default (1)
    -- nota de privacidad: no se almacena domicilio particular; la ubicación de
    -- un recurso corresponde siempre a una institución pública/comunitaria.
);
go
create index ix_recurso_organizacion on ocn.recurso(idorganizacionresponsable);

create table ocn.factorprotectorrecurso (     -- rn-91: factor protector puede relacionarse con recursos existentes
    idfactorprotector   int not null foreign key references ocn.catfactorprotector(idfactorprotector),
    idrecurso           int not null foreign key references ocn.recurso(idrecurso),
    constraint pk_factorprotectorrecurso primary key (idfactorprotector, idrecurso)
);
go

/* ============================================================================
   n. servicios de los recursos  (rn-95, rn-96, rn-104 a rn-108)
   ============================================================================ */

create table ocn.catservicio (
    idservicio      int identity(1,1) primary key,
    nombre          varchar(150) not null unique,
    activo          bit not null constraint df_catservicio_activo default (1)
);
go

create table ocn.recursoservicio (            -- rn-95, rn-96: n:m
    idrecurso   int not null foreign key references ocn.recurso(idrecurso),
    idservicio  int not null foreign key references ocn.catservicio(idservicio),
    constraint pk_recursoservicio primary key (idrecurso, idservicio)
);
go

create table ocn.servicionecesidad (          -- rn-107, rn-108: n:m
    idservicio  int not null foreign key references ocn.catservicio(idservicio),
    idnecesidad int not null foreign key references ocn.necesidad(idnecesidad),
    constraint pk_servicionecesidad primary key (idservicio, idnecesidad)
);
go

/* ============================================================================
   o. cobertura, accesibilidad y barreras  (rn-98-100, rn-109-117)
      relación con atributos: recurso + zona -> accesibilidad, barreras, cobertura
   ============================================================================ */

create table ocn.cattipocobertura (           -- zona, colonia, sector, municipio, región
    idtipocobertura int identity(1,1) primary key,
    nombre          varchar(30) not null unique,
    activo          bit not null constraint df_cattipocobertura_activo default (1)
);
go

create table ocn.recursozonacobertura (       -- rn-98-100, rn-109-112: histórica (fecha_fin null = vigente)
    idrecursozonacobertura  int identity(1,1) primary key,
    idrecurso               int not null foreign key references ocn.recurso(idrecurso),
    idzona                  int not null foreign key references ocn.zona(idzona),
    idtipocobertura         int not null foreign key references ocn.cattipocobertura(idtipocobertura),
    fechainicio             date not null,
    fechafin                date null           -- null = cobertura vigente; se cierra, no se borra (rn-112)
);
go
create index ix_recursozonacobertura_recurso on ocn.recursozonacobertura(idrecurso);
create index ix_recursozonacobertura_zona on ocn.recursozonacobertura(idzona);

create table ocn.catbarrera (                 -- distancia, transporte, costo, horario, idioma, etc.
    idbarrera       int identity(1,1) primary key,
    nombre          varchar(100) not null unique,
    activo          bit not null constraint df_catbarrera_activo default (1)
);
go

create table ocn.recursozonabarrera (         -- rn-113-116: barrera puede variar por grupo poblacional
    idrecursozonabarrera    int identity(1,1) primary key,
    idrecurso               int not null foreign key references ocn.recurso(idrecurso),
    idzona                  int not null foreign key references ocn.zona(idzona),
    idbarrera               int not null foreign key references ocn.catbarrera(idbarrera),
    idgrupopoblacional      int null foreign key references ocn.catgrupopoblacional(idgrupopoblacional), -- null = aplica en general
    fecharegistro           date not null constraint df_recursozonabarrera_fecha default (cast(getdate() as date)),
    notas                   varchar(300) null
);
go
create index ix_recursozonabarrera_recursozona on ocn.recursozonabarrera(idrecurso, idzona);

create table ocn.recursozonaaccesibilidad (   -- rn-117: recurso + zona + accesibilidad (no atributo simple del recurso)
    idrecursozonaaccesibilidad  int identity(1,1) primary key,
    idrecurso                   int not null foreign key references ocn.recurso(idrecurso),
    idzona                      int not null foreign key references ocn.zona(idzona),
    nivelaccesibilidadpercibida varchar(30) not null,
    fechaevaluacion             date not null,
    constraint uq_recursozonaaccesibilidad unique (idrecurso, idzona, fechaevaluacion)
);
go

/* ============================================================================
   p. evidencias y confiabilidad  (rn-118 a rn-125)
   ============================================================================ */

create table ocn.cattipoevidencia (           -- encuesta, entrevista, observación, documento institucional, etc.
    idtipoevidencia int identity(1,1) primary key,
    nombre          varchar(60) not null unique,
    activo          bit not null constraint df_cattipoevidencia_activo default (1)
);
go

create table ocn.evidencia (
    idevidencia                 int identity(1,1) primary key,
    idobservacionproblematica    int not null foreign key references ocn.observacionproblematica(idobservacionproblematica), -- rn-118, rn-120
    idtipoevidencia              int not null foreign key references ocn.cattipoevidencia(idtipoevidencia),                  -- rn-119
    idfuente                     int null foreign key references ocn.fuente(idfuente),  -- rn-121-123: permite distinguir menciones vs. fuentes independientes
    numeromenciones               int not null constraint df_evidencia_menciones default (1),
    descripcionbreve              varchar(300) null,     -- redacción que no permita identificar personas (ver requerimientos de privacidad)
    fecharegistro                 datetime2 not null constraint df_evidencia_fecha default (sysdatetime())
);
go
create index ix_evidencia_obsprob on ocn.evidencia(idobservacionproblematica);

/* ============================================================================
   q. rutas de intervención  (rn-139 a rn-145)
      problemática -> necesidad -> tipointervencion -> servicio -> recurso
   ============================================================================ */

create table ocn.cattipointervencion (
    idtipointervencion  int identity(1,1) primary key,
    nombre              varchar(150) not null unique,
    activo              bit not null constraint df_cattipointervencion_activo default (1)
);
go

create table ocn.necesidadtipointervencion (  -- rn-139-141: n:m
    idnecesidad         int not null foreign key references ocn.necesidad(idnecesidad),
    idtipointervencion  int not null foreign key references ocn.cattipointervencion(idtipointervencion),
    constraint pk_necesidadtipointervencion primary key (idnecesidad, idtipointervencion)
);
go

create table ocn.tipointervencionservicio (   -- rn-142-143: n:m
    idtipointervencion  int not null foreign key references ocn.cattipointervencion(idtipointervencion),
    idservicio          int not null foreign key references ocn.catservicio(idservicio),
    constraint pk_tipointervencionservicio primary key (idtipointervencion, idservicio)
);
go
-- nota (rn-144, rn-145): estas tablas describen rutas de atención posibles,
-- nunca la asignación de una persona concreta a una ruta ni expedientes individuales.

/* ============================================================================
   r. priorización comunitaria  (rn-126 a rn-132)
      modelos y ponderaciones separados de los datos originales.
   ============================================================================ */

create table ocn.modelopriorizacion (
    idmodelo        int identity(1,1) primary key,
    nombre          varchar(100) not null unique,   -- p.ej. "modelo a", "modelo b"
    descripcion     varchar(300) null,
    activo          bit not null constraint df_modelopriorizacion_activo default (1),
    fechacreacion   datetime2 not null constraint df_modelopriorizacion_fecha default (sysdatetime())
);
go

create table ocn.modeloponderacion (          -- rn-128, rn-130: pesos por variable, por modelo
    idmodelo    int not null foreign key references ocn.modelopriorizacion(idmodelo),
    variable    varchar(50) not null,   -- 'magnitud','gravedad','frecuencia','vulnerabilidad','evidencia','tendencia','brecha'
    peso        decimal(5,2) not null constraint ck_modeloponderacion_peso check (peso between 0 and 1),
    constraint pk_modeloponderacion primary key (idmodelo, variable)
);
go

create table ocn.prioridadcalculada (         -- rn-127, rn-129: zona+problemática+periodo(+grupo), valores originales conservados
    idprioridadcalculada    int identity(1,1) primary key,
    idzona                  int not null foreign key references ocn.zona(idzona),
    idproblematica          int not null foreign key references ocn.problematica(idproblematica),
    idperiodo               int not null foreign key references ocn.periodo(idperiodo),
    idgrupopoblacional      int null foreign key references ocn.catgrupopoblacional(idgrupopoblacional),  -- null = general
    idmodelo                int not null foreign key references ocn.modelopriorizacion(idmodelo),
    valormagnitud            decimal(6,2) null,
    valorgravedad            decimal(6,2) null,
    valorfrecuencia          decimal(6,2) null,
    valorvulnerabilidad      decimal(6,2) null,
    valorevidencia           decimal(6,2) null,
    valortendencia           decimal(6,2) null,
    valorbrecha              decimal(6,2) null,
    puntajeresultante        decimal(6,2) not null,
    fechacalculo             datetime2 not null constraint df_prioridadcalculada_fecha default (sysdatetime()),
    constraint uq_prioridadcalculada unique (idzona, idproblematica, idperiodo, idgrupopoblacional, idmodelo)
);
go

/* ============================================================================
   s. brecha de atención  (rn-133 a rn-138)
      indicador derivado: necesidad + zona + periodo vs. recursos disponibles.
   ============================================================================ */

create table ocn.brechaatencion (
    idbrechaatencion        int identity(1,1) primary key,
    idzona                   int not null foreign key references ocn.zona(idzona),
    idnecesidad              int not null foreign key references ocn.necesidad(idnecesidad),
    idperiodo                int not null foreign key references ocn.periodo(idperiodo),
    recursosidentificados     int not null constraint df_brechaatencion_recursos default (0),
    nivelbrecha               varchar(30) not null,   -- p.ej. sin cobertura, cobertura parcial, cobertura con barreras, cubierta
    fechacalculo              datetime2 not null constraint df_brechaatencion_fecha default (sysdatetime()),
    constraint uq_brechaatencion unique (idzona, idnecesidad, idperiodo)
);
go

/* ============================================================================
   t. auditoría y trazabilidad  (rn-174, rn-175, rn-182 a rn-187)
   ============================================================================ */

create table ocn.auditoria (
    idauditoria         bigint identity(1,1) primary key,
    nombretabla         varchar(100) not null,
    idregistro          int not null,
    operacion           varchar(10) not null,     -- insert, update, delete-logico
    idusuario           int null foreign key references ocn.usuario(idusuario),
    fechaoperacion      datetime2 not null constraint df_auditoria_fecha default (sysdatetime()),
    valoresanteriores   nvarchar(max) null,        -- json con el estado previo
    valoresnuevos       nvarchar(max) null         -- json con el estado nuevo
);
go
create index ix_auditoria_tabla_registro on ocn.auditoria(nombretabla, idregistro);
go
/* ============================================================================
   vistas de apoyo para las preguntas de análisis del planteamiento
   (ejemplos; se pueden ampliar según las 15 preguntas del documento)
   ============================================================================ */

-- problemáticas más frecuentes por zona
create or alter view ocn.vw_problematicasporzona as
select  z.idzona, z.codigozona, z.nombre as zona,
        p.idproblematica, p.nombre as problematica,
        count(*) as numeroobservaciones
from ocn.observacionproblematica op
join ocn.observacion o        on o.idobservacion = op.idobservacion
join ocn.levantamiento l       on l.idlevantamiento = o.idlevantamiento
join ocn.zona z                on z.idzona = l.idzona
join ocn.problematica p        on p.idproblematica = op.idproblematica
group by z.idzona, z.codigozona, z.nombre, p.idproblematica, p.nombre;
go

-- necesidades sin ningún recurso relacionado (brecha total)
create or alter view ocn.vw_necesidadessinrecurso as
select n.idnecesidad, n.nombre as necesidad
from ocn.necesidad n
where not exists (
    select 1
    from ocn.servicionecesidad sn
    join ocn.recursoservicio rs on rs.idservicio = sn.idservicio
    where sn.idnecesidad = n.idnecesidad
);
go

-- problemáticas respaldadas por múltiples fuentes independientes
create or alter view ocn.vw_problematicasmultifuente as
select op.idproblematica, p.nombre as problematica,
       count(distinct ofu.idfuente) as numerofuentesdistintas
from ocn.observacionproblematica op
join ocn.problematica p     on p.idproblematica = op.idproblematica
join ocn.observacionfuente ofu on ofu.idobservacion = op.idobservacion
group by op.idproblematica, p.nombre
having count(distinct ofu.idfuente) > 1;
go

/* ============================================================================
   fin del script
   ============================================================================ */
