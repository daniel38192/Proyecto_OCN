using Microsoft.EntityFrameworkCore;
using OCN_Project.Models.Entities;

namespace OCN_Project.Data;

/// <summary>
/// DbContext del proyecto OCN. Mapea el esquema ocn de la base proyecto_ocn
/// (database-first: la base ya existe, no se generan migraciones).
/// </summary>
public class OcnDbContext : DbContext
{
    public OcnDbContext(DbContextOptions<OcnDbContext> options) : base(options) { }

    // ---- seguridad ----
    public DbSet<Rol> Roles { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<UsuarioRol> UsuariosRoles { get; set; } = null!;

    // ---- territorio ----
    public DbSet<DivisionTerritorial> DivisionesTerritoriales { get; set; } = null!;
    public DbSet<CatTipoZona> TiposZona { get; set; } = null!;
    public DbSet<Zona> Zonas { get; set; } = null!;

    // ---- periodos ----
    public DbSet<Periodo> Periodos { get; set; } = null!;

    // ---- métodos e instrumentos ----
    public DbSet<CatMetodo> Metodos { get; set; } = null!;
    public DbSet<Instrumento> Instrumentos { get; set; } = null!;

    // ---- levantamientos ----
    public DbSet<Levantamiento> Levantamientos { get; set; } = null!;
    public DbSet<LevantamientoInstrumento> LevantamientosInstrumentos { get; set; } = null!;
    public DbSet<LevantamientoResponsable> LevantamientosResponsables { get; set; } = null!;
    public DbSet<LevantamientoFuente> LevantamientosFuentes { get; set; } = null!;

    // ---- fuentes ----
    public DbSet<CatTipoFuente> TiposFuente { get; set; } = null!;
    public DbSet<CatTipoOrganizacion> TiposOrganizacion { get; set; } = null!;
    public DbSet<Organizacion> Organizaciones { get; set; } = null!;
    public DbSet<Fuente> Fuentes { get; set; } = null!;

    // ---- observaciones ----
    public DbSet<Observacion> Observaciones { get; set; } = null!;
    public DbSet<ObservacionFuente> ObservacionesFuentes { get; set; } = null!;
    public DbSet<CatGrupoPoblacional> GruposPoblacionales { get; set; } = null!;
    public DbSet<ObservacionGrupoPoblacional> ObservacionesGruposPoblacional { get; set; } = null!;

    // ---- problemáticas y escalas ----
    public DbSet<CategoriaProblematica> CategoriasProblematica { get; set; } = null!;
    public DbSet<Problematica> Problematicas { get; set; } = null!;
    public DbSet<CatMagnitud> Magnitudes { get; set; } = null!;
    public DbSet<CatGravedad> Gravedades { get; set; } = null!;
    public DbSet<CatFrecuencia> Frecuencias { get; set; } = null!;
    public DbSet<CatTendencia> Tendencias { get; set; } = null!;
    public DbSet<ObservacionProblematica> ObservacionesProblematica { get; set; } = null!;
    public DbSet<TendenciaProblematica> TendenciasProblematica { get; set; } = null!;

    // ---- necesidades y factores ----
    public DbSet<Necesidad> Necesidades { get; set; } = null!;
    public DbSet<ProblematicaNecesidad> ProblematicasNecesidades { get; set; } = null!;
    public DbSet<CatFactorRiesgo> FactoresRiesgo { get; set; } = null!;
    public DbSet<ProblematicaFactorRiesgo> ProblematicasFactoresRiesgo { get; set; } = null!;
    public DbSet<CatFactorProtector> FactoresProtector { get; set; } = null!;
    public DbSet<ProblematicaFactorProtector> ProblematicasFactoresProtector { get; set; } = null!;

    // ---- recursos ----
    public DbSet<CatTipoRecurso> TiposRecurso { get; set; } = null!;
    public DbSet<CatEstadoRecurso> EstadosRecurso { get; set; } = null!;
    public DbSet<CatCostoRecurso> CostosRecurso { get; set; } = null!;
    public DbSet<Recurso> Recursos { get; set; } = null!;
    public DbSet<CatServicio> Servicios { get; set; } = null!;
    public DbSet<RecursoServicio> RecursosServicios { get; set; } = null!;
    public DbSet<ServicioNecesidad> ServiciosNecesidades { get; set; } = null!;
    public DbSet<CatTipoCobertura> TiposCobertura { get; set; } = null!;
    public DbSet<RecursoZonaCobertura> RecursosZonasCobertura { get; set; } = null!;
    public DbSet<CatBarrera> Barreras { get; set; } = null!;
    public DbSet<RecursoZonaBarrera> RecursosZonasBarrera { get; set; } = null!;
    public DbSet<RecursoZonaAccesibilidad> RecursosZonasAccesibilidad { get; set; } = null!;

    // ---- evidencias ----
    public DbSet<CatTipoEvidencia> TiposEvidencia { get; set; } = null!;
    public DbSet<Evidencia> Evidencias { get; set; } = null!;

    // ---- intervención ----
    public DbSet<CatTipoIntervencion> TiposIntervencion { get; set; } = null!;
    public DbSet<NecesidadTipoIntervencion> NecesidadesTiposIntervencion { get; set; } = null!;
    public DbSet<TipoIntervencionServicio> TiposIntervencionServicios { get; set; } = null!;

    // ---- priorización y brecha ----
    public DbSet<ModeloPriorizacion> ModelosPriorizacion { get; set; } = null!;
    public DbSet<ModeloPonderacion> ModelosPonderacion { get; set; } = null!;
    public DbSet<PrioridadCalculada> PrioridadesCalculada { get; set; } = null!;
    public DbSet<BrechaAtencion> BrechasAtencion { get; set; } = null!;

    // ---- auditoría ----
    public DbSet<Auditoria> Auditorias { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ocn");

        // ---------- llaves primarias simples ----------
        modelBuilder.Entity<Rol>().HasKey(x => x.IdRol);
        modelBuilder.Entity<Usuario>().HasKey(x => x.IdUsuario);
        modelBuilder.Entity<DivisionTerritorial>().HasKey(x => x.IdDivision);
        modelBuilder.Entity<CatTipoZona>().HasKey(x => x.IdTipoZona);
        modelBuilder.Entity<Zona>().HasKey(x => x.IdZona);
        modelBuilder.Entity<Periodo>().HasKey(x => x.IdPeriodo);
        modelBuilder.Entity<CatMetodo>().HasKey(x => x.IdMetodo);
        modelBuilder.Entity<Instrumento>().HasKey(x => x.IdInstrumento);
        modelBuilder.Entity<Levantamiento>().HasKey(x => x.IdLevantamiento);
        modelBuilder.Entity<CatTipoFuente>().HasKey(x => x.IdTipoFuente);
        modelBuilder.Entity<CatTipoOrganizacion>().HasKey(x => x.IdTipoOrganizacion);
        modelBuilder.Entity<Organizacion>().HasKey(x => x.IdOrganizacion);
        modelBuilder.Entity<Fuente>().HasKey(x => x.IdFuente);
        modelBuilder.Entity<Observacion>().HasKey(x => x.IdObservacion);
        modelBuilder.Entity<CatGrupoPoblacional>().HasKey(x => x.IdGrupoPoblacional);
        modelBuilder.Entity<CategoriaProblematica>().HasKey(x => x.IdCategoria);
        modelBuilder.Entity<Problematica>().HasKey(x => x.IdProblematica);
        modelBuilder.Entity<CatMagnitud>().HasKey(x => x.IdMagnitud);
        modelBuilder.Entity<CatGravedad>().HasKey(x => x.IdGravedad);
        modelBuilder.Entity<CatFrecuencia>().HasKey(x => x.IdFrecuencia);
        modelBuilder.Entity<CatTendencia>().HasKey(x => x.IdTendencia);
        modelBuilder.Entity<ObservacionProblematica>().HasKey(x => x.IdObservacionProblematica);
        modelBuilder.Entity<TendenciaProblematica>().HasKey(x => x.IdTendenciaProblematica);
        modelBuilder.Entity<Necesidad>().HasKey(x => x.IdNecesidad);
        modelBuilder.Entity<CatFactorRiesgo>().HasKey(x => x.IdFactorRiesgo);
        modelBuilder.Entity<CatFactorProtector>().HasKey(x => x.IdFactorProtector);
        modelBuilder.Entity<CatTipoRecurso>().HasKey(x => x.IdTipoRecurso);
        modelBuilder.Entity<CatEstadoRecurso>().HasKey(x => x.IdEstadoRecurso);
        modelBuilder.Entity<CatCostoRecurso>().HasKey(x => x.IdCostoRecurso);
        modelBuilder.Entity<Recurso>().HasKey(x => x.IdRecurso);
        modelBuilder.Entity<CatServicio>().HasKey(x => x.IdServicio);
        modelBuilder.Entity<CatTipoCobertura>().HasKey(x => x.IdTipoCobertura);
        modelBuilder.Entity<RecursoZonaCobertura>().HasKey(x => x.IdRecursoZonaCobertura);
        modelBuilder.Entity<CatBarrera>().HasKey(x => x.IdBarrera);
        modelBuilder.Entity<RecursoZonaBarrera>().HasKey(x => x.IdRecursoZonaBarrera);
        modelBuilder.Entity<RecursoZonaAccesibilidad>().HasKey(x => x.IdRecursoZonaAccesibilidad);
        modelBuilder.Entity<CatTipoEvidencia>().HasKey(x => x.IdTipoEvidencia);
        modelBuilder.Entity<Evidencia>().HasKey(x => x.IdEvidencia);
        modelBuilder.Entity<CatTipoIntervencion>().HasKey(x => x.IdTipoIntervencion);
        modelBuilder.Entity<ModeloPriorizacion>().HasKey(x => x.IdModelo);
        modelBuilder.Entity<PrioridadCalculada>().HasKey(x => x.IdPrioridadCalculada);
        modelBuilder.Entity<BrechaAtencion>().HasKey(x => x.IdBrechaAtencion);
        modelBuilder.Entity<Auditoria>().HasKey(x => x.IdAuditoria);
        modelBuilder.Entity<Auditoria>().Property(x => x.IdAuditoria).ValueGeneratedOnAdd();

        // ---------- llaves compuestas (tablas puente) ----------
        modelBuilder.Entity<UsuarioRol>().HasKey(x => new { x.IdUsuario, x.IdRol });
        modelBuilder.Entity<LevantamientoInstrumento>().HasKey(x => new { x.IdLevantamiento, x.IdInstrumento });
        modelBuilder.Entity<LevantamientoResponsable>().HasKey(x => new { x.IdLevantamiento, x.IdUsuario });
        modelBuilder.Entity<LevantamientoFuente>().HasKey(x => new { x.IdLevantamiento, x.IdFuente });
        modelBuilder.Entity<ObservacionFuente>().HasKey(x => new { x.IdObservacion, x.IdFuente });
        modelBuilder.Entity<ObservacionGrupoPoblacional>().HasKey(x => new { x.IdObservacion, x.IdGrupoPoblacional });
        modelBuilder.Entity<ProblematicaNecesidad>().HasKey(x => new { x.IdProblematica, x.IdNecesidad });
        modelBuilder.Entity<ProblematicaFactorRiesgo>().HasKey(x => new { x.IdProblematica, x.IdFactorRiesgo });
        modelBuilder.Entity<ProblematicaFactorProtector>().HasKey(x => new { x.IdProblematica, x.IdFactorProtector });
        modelBuilder.Entity<FactorProtectorRecurso>().HasKey(x => new { x.IdFactorProtector, x.IdRecurso });
        modelBuilder.Entity<RecursoServicio>().HasKey(x => new { x.IdRecurso, x.IdServicio });
        modelBuilder.Entity<ServicioNecesidad>().HasKey(x => new { x.IdServicio, x.IdNecesidad });
        modelBuilder.Entity<NecesidadTipoIntervencion>().HasKey(x => new { x.IdNecesidad, x.IdTipoIntervencion });
        modelBuilder.Entity<TipoIntervencionServicio>().HasKey(x => new { x.IdTipoIntervencion, x.IdServicio });
        modelBuilder.Entity<ModeloPonderacion>().HasKey(x => new { x.IdModelo, x.Variable });

        // ---------- autorreferencias ----------
        modelBuilder.Entity<Instrumento>()
            .HasOne(x => x.InstrumentoAnterior)
            .WithMany(x => x.VersionesPosteriores)
            .HasForeignKey(x => x.IdInstrumentoAnterior);

        modelBuilder.Entity<Problematica>()
            .HasOne(x => x.ProblematicaPadre)
            .WithMany(x => x.ProblematicasHijas)
            .HasForeignKey(x => x.IdProblematicaPadre);

        // ---------- precisión decimal ----------
        modelBuilder.Entity<ObservacionProblematica>().Property(x => x.NivelConfianza).HasPrecision(5, 2);
        modelBuilder.Entity<ModeloPonderacion>().Property(x => x.Peso).HasPrecision(5, 2);
        modelBuilder.Entity<PrioridadCalculada>().Property(x => x.PuntajeResultante).HasPrecision(6, 2);
        modelBuilder.Entity<PrioridadCalculada>().Property(x => x.ValorMagnitud).HasPrecision(6, 2);
        modelBuilder.Entity<PrioridadCalculada>().Property(x => x.ValorGravedad).HasPrecision(6, 2);
        modelBuilder.Entity<PrioridadCalculada>().Property(x => x.ValorFrecuencia).HasPrecision(6, 2);
        modelBuilder.Entity<PrioridadCalculada>().Property(x => x.ValorVulnerabilidad).HasPrecision(6, 2);
        modelBuilder.Entity<PrioridadCalculada>().Property(x => x.ValorEvidencia).HasPrecision(6, 2);
        modelBuilder.Entity<PrioridadCalculada>().Property(x => x.ValorTendencia).HasPrecision(6, 2);
        modelBuilder.Entity<PrioridadCalculada>().Property(x => x.ValorBrecha).HasPrecision(6, 2);

        // ---------- mapeo entidad -> tabla física (esquema ocn) ----------
        modelBuilder.Entity<Rol>().ToTable("rol");
        modelBuilder.Entity<Usuario>().ToTable("usuario");
        modelBuilder.Entity<UsuarioRol>().ToTable("usuariorol");
        modelBuilder.Entity<DivisionTerritorial>().ToTable("divisionterritorial");
        modelBuilder.Entity<CatTipoZona>().ToTable("cattipozona");
        modelBuilder.Entity<Zona>().ToTable("zona");
        modelBuilder.Entity<Periodo>().ToTable("periodo");
        modelBuilder.Entity<CatMetodo>().ToTable("catmetodo");
        modelBuilder.Entity<Instrumento>().ToTable("instrumento");
        modelBuilder.Entity<Levantamiento>().ToTable("levantamiento");
        modelBuilder.Entity<LevantamientoInstrumento>().ToTable("levantamientoinstrumento");
        modelBuilder.Entity<LevantamientoResponsable>().ToTable("levantamientoresponsable");
        modelBuilder.Entity<LevantamientoFuente>().ToTable("levantamientofuente");
        modelBuilder.Entity<CatTipoFuente>().ToTable("cattipofuente");
        modelBuilder.Entity<CatTipoOrganizacion>().ToTable("cattipoorganizacion");
        modelBuilder.Entity<Organizacion>().ToTable("organizacion");
        modelBuilder.Entity<Fuente>().ToTable("fuente");
        modelBuilder.Entity<Observacion>().ToTable("observacion");
        modelBuilder.Entity<ObservacionFuente>().ToTable("observacionfuente");
        modelBuilder.Entity<CatGrupoPoblacional>().ToTable("catgrupopoblacional");
        modelBuilder.Entity<ObservacionGrupoPoblacional>().ToTable("observaciongrupopoblacional");
        modelBuilder.Entity<CategoriaProblematica>().ToTable("categoriaproblematica");
        modelBuilder.Entity<Problematica>().ToTable("problematica");
        modelBuilder.Entity<CatMagnitud>().ToTable("catmagnitud");
        modelBuilder.Entity<CatGravedad>().ToTable("catgravedad");
        modelBuilder.Entity<CatFrecuencia>().ToTable("catfrecuencia");
        modelBuilder.Entity<CatTendencia>().ToTable("cattendencia");
        modelBuilder.Entity<ObservacionProblematica>().ToTable("observacionproblematica");
        modelBuilder.Entity<TendenciaProblematica>().ToTable("tendenciaproblematica");
        modelBuilder.Entity<Necesidad>().ToTable("necesidad");
        modelBuilder.Entity<ProblematicaNecesidad>().ToTable("problematicanecesidad");
        modelBuilder.Entity<CatFactorRiesgo>().ToTable("catfactorriesgo");
        modelBuilder.Entity<ProblematicaFactorRiesgo>().ToTable("problematicafactorriesgo");
        modelBuilder.Entity<CatFactorProtector>().ToTable("catfactorprotector");
        modelBuilder.Entity<ProblematicaFactorProtector>().ToTable("problematicafactorprotector");
        modelBuilder.Entity<CatTipoRecurso>().ToTable("cattiporecurso");
        modelBuilder.Entity<CatEstadoRecurso>().ToTable("catestadorecurso");
        modelBuilder.Entity<CatCostoRecurso>().ToTable("catcostorecurso");
        modelBuilder.Entity<Recurso>().ToTable("recurso");
        modelBuilder.Entity<FactorProtectorRecurso>().ToTable("factorprotectorrecurso");
        modelBuilder.Entity<CatServicio>().ToTable("catservicio");
        modelBuilder.Entity<RecursoServicio>().ToTable("recursoservicio");
        modelBuilder.Entity<ServicioNecesidad>().ToTable("servicionecesidad");
        modelBuilder.Entity<CatTipoCobertura>().ToTable("cattipocobertura");
        modelBuilder.Entity<RecursoZonaCobertura>().ToTable("recursozonacobertura");
        modelBuilder.Entity<CatBarrera>().ToTable("catbarrera");
        modelBuilder.Entity<RecursoZonaBarrera>().ToTable("recursozonabarrera");
        modelBuilder.Entity<RecursoZonaAccesibilidad>().ToTable("recursozonaaccesibilidad");
        modelBuilder.Entity<CatTipoEvidencia>().ToTable("cattipoevidencia");
        modelBuilder.Entity<Evidencia>().ToTable("evidencia");
        modelBuilder.Entity<CatTipoIntervencion>().ToTable("cattipointervencion");
        modelBuilder.Entity<NecesidadTipoIntervencion>().ToTable("necesidadtipointervencion");
        modelBuilder.Entity<TipoIntervencionServicio>().ToTable("tipointervencionservicio");
        modelBuilder.Entity<ModeloPriorizacion>().ToTable("modelopriorizacion");
        modelBuilder.Entity<ModeloPonderacion>().ToTable("modeloponderacion");
        modelBuilder.Entity<PrioridadCalculada>().ToTable("prioridadcalculada");
        modelBuilder.Entity<BrechaAtencion>().ToTable("brechaatencion");
        modelBuilder.Entity<Auditoria>().ToTable("auditoria");

        // ---------- comportamiento de borrado ----------
        // La base ya existe con sus own FKs; SQL Server rechaza múltiples rutas
        // de borrado en cascada, así que todas las FKs quedan en Restrict (NoAction),
        // igual que lo genera el scaffolding de una base database-first.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        base.OnModelCreating(modelBuilder);
    }
}
