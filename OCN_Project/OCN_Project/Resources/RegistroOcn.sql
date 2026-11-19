/* ============================================================================
   Tabla del módulo RegistrosOCN (módulo principal de la WebApp MVC).
   FKs hacia zona y problematica; intensidad y frecuencia en rango 1-5
   (validación reforzada en la aplicación con Data Annotations [Range(1,5)]).
   Incluye borrado lógico: columna Activo (1 = activo, 0 = eliminado lógicamente).
   Script idempotente.
   ============================================================================ */

use proyecto_ocn
go

if not exists (select 1 from sys.tables where schema_id = SCHEMA_ID('ocn') and name = 'registroocn')
begin
    create table ocn.registroocn (
        idregistroocn   int identity(1,1) primary key,
        idzona          int not null foreign key references ocn.zona(idzona),
        idproblematica  int not null foreign key references ocn.problematica(idproblematica),
        intensidad      int not null,
        frecuencia      int not null,
        notacontexto    varchar(500) null,
        fecharegistro   datetime2 not null constraint df_registroocn_fecharegistro default (sysdatetime()),
        activo          bit not null constraint df_registroocn_activo default (1)
    );
end
go

-- Idempotencia: agregar columna Activo si la tabla ya existía sin ella
if exists (select 1 from sys.tables where schema_id = SCHEMA_ID('ocn') and name = 'registroocn')
   and not exists (select 1 from sys.columns where object_id = OBJECT_ID('ocn.registroocn') and name = 'activo')
begin
    alter table ocn.registroocn add activo bit not null constraint df_registroocn_activo default (1);
end
go

if not exists (select 1 from sys.indexes where name = 'ix_registroocn_zona' and object_id = OBJECT_ID('ocn.registroocn'))
    create index ix_registroocn_zona on ocn.registroocn(idzona);
go

if not exists (select 1 from sys.indexes where name = 'ix_registroocn_problematica' and object_id = OBJECT_ID('ocn.registroocn'))
    create index ix_registroocn_problematica on ocn.registroocn(idproblematica);
go
