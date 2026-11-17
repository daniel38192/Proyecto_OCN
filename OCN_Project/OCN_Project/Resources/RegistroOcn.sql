/* ============================================================================
   Tabla del módulo RegistrosOCN (módulo principal de la WebApp MVC).
   FKs hacia zona y problematica; intensidad y frecuencia en rango 1-10
   (la validación de rango se refuerza en la aplicación con Data Annotations).
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
        fecharegistro   datetime2 not null constraint df_registroocn_fecharegistro default (sysdatetime())
    );
end
go

create index ix_registroocn_zona on ocn.registroocn(idzona);
create index ix_registroocn_problematica on ocn.registroocn(idproblematica);
go
