using Microsoft.EntityFrameworkCore;
using OCN_Project.Models.Entities;

namespace OCN_Project.Data;

/// <summary>
/// DbContext del proyecto OCN. Mapea el esquema ocn de la base proyecto_ocn
/// (database-first: la base ya existe, no se generan migraciones).
/// Todas las relaciones se declaran explícitamente para que EF use las
/// columnas FK existentes (idzona, idproblematica, etc.).
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

    // ---- módulo registros OCN ----
    public DbSet<RegistroOcn> RegistrosOcn { get; set; } = null!;

    // ---- auditoría ----
    public DbSet<Auditoria> Auditorias { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ocn");

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
        modelBuilder.Entity<RegistroOcn>().ToTable("registroocn");
        modelBuilder.Entity<Auditoria>().ToTable("auditoria");

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

        // ---------- relaciones: seguridad ----------
        modelBuilder.Entity<UsuarioRol>()
            .HasOne(x => x.Usuario).WithMany(u => u.UsuarioRoles).HasForeignKey(x => x.IdUsuario);
        modelBuilder.Entity<UsuarioRol>()
            .HasOne(x => x.Rol).WithMany(r => r.UsuarioRoles).HasForeignKey(x => x.IdRol);

        // ---------- relaciones: territorio ----------
        modelBuilder.Entity<Zona>()
            .HasOne(x => x.Division).WithMany(d => d.Zonas).HasForeignKey(x => x.IdDivision);
        modelBuilder.Entity<Zona>()
            .HasOne(x => x.TipoZona).WithMany(t => t.Zonas).HasForeignKey(x => x.IdTipoZona);

        // ---------- relaciones: métodos e instrumentos ----------
        modelBuilder.Entity<Instrumento>()
            .HasOne(x => x.Metodo).WithMany(m => m.Instrumentos).HasForeignKey(x => x.IdMetodo);
        modelBuilder.Entity<Instrumento>()
            .HasOne(x => x.InstrumentoAnterior).WithMany(x => x.VersionesPosteriores)
            .HasForeignKey(x => x.IdInstrumentoAnterior);

        // ---------- relaciones: levantamientos ----------
        modelBuilder.Entity<Levantamiento>()
            .HasOne(x => x.Zona).WithMany(z => z.Levantamientos).HasForeignKey(x => x.IdZona);
        modelBuilder.Entity<Levantamiento>()
            .HasOne(x => x.Periodo).WithMany(p => p.Levantamientos).HasForeignKey(x => x.IdPeriodo);
        modelBuilder.Entity<Levantamiento>()
            .HasOne(x => x.Metodo).WithMany(m => m.Levantamientos).HasForeignKey(x => x.IdMetodo);
        modelBuilder.Entity<LevantamientoInstrumento>()
            .HasOne(x => x.Levantamiento).WithMany(l => l.LevantamientoInstrumentos).HasForeignKey(x => x.IdLevantamiento);
        modelBuilder.Entity<LevantamientoInstrumento>()
            .HasOne(x => x.Instrumento).WithMany(i => i.LevantamientoInstrumentos).HasForeignKey(x => x.IdInstrumento);
        modelBuilder.Entity<LevantamientoResponsable>()
            .HasOne(x => x.Levantamiento).WithMany(l => l.LevantamientoResponsables).HasForeignKey(x => x.IdLevantamiento);
        modelBuilder.Entity<LevantamientoResponsable>()
            .HasOne(x => x.Usuario).WithMany(u => u.LevantamientoResponsables).HasForeignKey(x => x.IdUsuario);
        modelBuilder.Entity<LevantamientoFuente>()
            .HasOne(x => x.Levantamiento).WithMany(l => l.LevantamientoFuentes).HasForeignKey(x => x.IdLevantamiento);
        modelBuilder.Entity<LevantamientoFuente>()
            .HasOne(x => x.Fuente).WithMany(f => f.LevantamientoFuentes).HasForeignKey(x => x.IdFuente);

        // ---------- relaciones: fuentes y organizaciones ----------
        modelBuilder.Entity<Organizacion>()
            .HasOne(x => x.TipoOrganizacion).WithMany(t => t.Organizaciones).HasForeignKey(x => x.IdTipoOrganizacion);
        modelBuilder.Entity<Fuente>()
            .HasOne(x => x.TipoFuente).WithMany(t => t.Fuentes).HasForeignKey(x => x.IdTipoFuente);
        modelBuilder.Entity<Fuente>()
            .HasOne(x => x.Organizacion).WithMany(o => o.Fuentes).HasForeignKey(x => x.IdOrganizacion);

        // ---------- relaciones: observaciones y grupos poblacionales ----------
        modelBuilder.Entity<Observacion>()
            .HasOne(x => x.Levantamiento).WithMany(l => l.Observaciones).HasForeignKey(x => x.IdLevantamiento);
        modelBuilder.Entity<ObservacionFuente>()
            .HasOne(x => x.Observacion).WithMany(o => o.ObservacionFuentes).HasForeignKey(x => x.IdObservacion);
        modelBuilder.Entity<ObservacionFuente>()
            .HasOne(x => x.Fuente).WithMany(f => f.ObservacionFuentes).HasForeignKey(x => x.IdFuente);
        modelBuilder.Entity<ObservacionGrupoPoblacional>()
            .HasOne(x => x.Observacion).WithMany(o => o.ObservacionGruposPoblacional).HasForeignKey(x => x.IdObservacion);
        modelBuilder.Entity<ObservacionGrupoPoblacional>()
            .HasOne(x => x.GrupoPoblacional).WithMany(g => g.ObservacionGruposPoblacional).HasForeignKey(x => x.IdGrupoPoblacional);

        // ---------- relaciones: problemáticas ----------
        modelBuilder.Entity<Problematica>()
            .HasOne(x => x.Categoria).WithMany(c => c.Problematicas).HasForeignKey(x => x.IdCategoria);
        modelBuilder.Entity<Problematica>()
            .HasOne(x => x.ProblematicaPadre).WithMany(x => x.ProblematicasHijas)
            .HasForeignKey(x => x.IdProblematicaPadre);

        // ---------- relaciones: escalas y hecho central ----------
        modelBuilder.Entity<ObservacionProblematica>()
            .HasOne(x => x.Observacion).WithMany(o => o.ObservacionesProblematica).HasForeignKey(x => x.IdObservacion);
        modelBuilder.Entity<ObservacionProblematica>()
            .HasOne(x => x.Problematica).WithMany(p => p.ObservacionesProblematica).HasForeignKey(x => x.IdProblematica);
        modelBuilder.Entity<ObservacionProblematica>()
            .HasOne(x => x.Magnitud).WithMany(m => m.ObservacionesProblematica).HasForeignKey(x => x.IdMagnitud);
        modelBuilder.Entity<ObservacionProblematica>()
            .HasOne(x => x.Gravedad).WithMany(g => g.ObservacionesProblematica).HasForeignKey(x => x.IdGravedad);
        modelBuilder.Entity<ObservacionProblematica>()
            .HasOne(x => x.Frecuencia).WithMany(f => f.ObservacionesProblematica).HasForeignKey(x => x.IdFrecuencia);
        modelBuilder.Entity<TendenciaProblematica>()
            .HasOne(x => x.Zona).WithMany(z => z.TendenciasProblematica).HasForeignKey(x => x.IdZona);
        modelBuilder.Entity<TendenciaProblematica>()
            .HasOne(x => x.Problematica).WithMany(p => p.TendenciasProblematica).HasForeignKey(x => x.IdProblematica);
        modelBuilder.Entity<TendenciaProblematica>()
            .HasOne(x => x.Periodo).WithMany(p => p.TendenciasProblematica).HasForeignKey(x => x.IdPeriodo);
        modelBuilder.Entity<TendenciaProblematica>()
            .HasOne(x => x.Tendencia).WithMany(t => t.TendenciasProblematica).HasForeignKey(x => x.IdTendencia);

        // ---------- relaciones: necesidades y factores ----------
        modelBuilder.Entity<ProblematicaNecesidad>()
            .HasOne(x => x.Problematica).WithMany(p => p.ProblematicaNecesidades).HasForeignKey(x => x.IdProblematica);
        modelBuilder.Entity<ProblematicaNecesidad>()
            .HasOne(x => x.Necesidad).WithMany(n => n.ProblematicaNecesidades).HasForeignKey(x => x.IdNecesidad);
        modelBuilder.Entity<ProblematicaFactorRiesgo>()
            .HasOne(x => x.Problematica).WithMany(p => p.ProblematicaFactoresRiesgo).HasForeignKey(x => x.IdProblematica);
        modelBuilder.Entity<ProblematicaFactorRiesgo>()
            .HasOne(x => x.FactorRiesgo).WithMany(f => f.ProblematicaFactoresRiesgo).HasForeignKey(x => x.IdFactorRiesgo);
        modelBuilder.Entity<ProblematicaFactorProtector>()
            .HasOne(x => x.Problematica).WithMany(p => p.ProblematicaFactoresProtector).HasForeignKey(x => x.IdProblematica);
        modelBuilder.Entity<ProblematicaFactorProtector>()
            .HasOne(x => x.FactorProtector).WithMany(f => f.ProblematicaFactoresProtector).HasForeignKey(x => x.IdFactorProtector);

        // ---------- relaciones: recursos y servicios ----------
        modelBuilder.Entity<Recurso>()
            .HasOne(x => x.TipoRecurso).WithMany(t => t.Recursos).HasForeignKey(x => x.IdTipoRecurso);
        modelBuilder.Entity<Recurso>()
            .HasOne(x => x.OrganizacionResponsable).WithMany(o => o.Recursos).HasForeignKey(x => x.IdOrganizacionResponsable);
        modelBuilder.Entity<Recurso>()
            .HasOne(x => x.EstadoRecurso).WithMany(e => e.Recursos).HasForeignKey(x => x.IdEstadoRecurso);
        modelBuilder.Entity<Recurso>()
            .HasOne(x => x.CostoRecurso).WithMany(c => c.Recursos).HasForeignKey(x => x.IdCostoRecurso);
        modelBuilder.Entity<FactorProtectorRecurso>()
            .HasOne(x => x.FactorProtector).WithMany(f => f.FactorProtectorRecursos).HasForeignKey(x => x.IdFactorProtector);
        modelBuilder.Entity<FactorProtectorRecurso>()
            .HasOne(x => x.Recurso).WithMany(r => r.FactorProtectorRecursos).HasForeignKey(x => x.IdRecurso);
        modelBuilder.Entity<RecursoServicio>()
            .HasOne(x => x.Recurso).WithMany(r => r.RecursoServicios).HasForeignKey(x => x.IdRecurso);
        modelBuilder.Entity<RecursoServicio>()
            .HasOne(x => x.Servicio).WithMany(s => s.RecursoServicios).HasForeignKey(x => x.IdServicio);
        modelBuilder.Entity<ServicioNecesidad>()
            .HasOne(x => x.Servicio).WithMany(s => s.ServicioNecesidades).HasForeignKey(x => x.IdServicio);
        modelBuilder.Entity<ServicioNecesidad>()
            .HasOne(x => x.Necesidad).WithMany(n => n.ServicioNecesidades).HasForeignKey(x => x.IdNecesidad);

        // ---------- relaciones: cobertura, barreras y accesibilidad ----------
        modelBuilder.Entity<RecursoZonaCobertura>()
            .HasOne(x => x.Recurso).WithMany(r => r.RecursoZonaCoberturas).HasForeignKey(x => x.IdRecurso);
        modelBuilder.Entity<RecursoZonaCobertura>()
            .HasOne(x => x.Zona).WithMany(z => z.RecursoZonaCoberturas).HasForeignKey(x => x.IdZona);
        modelBuilder.Entity<RecursoZonaCobertura>()
            .HasOne(x => x.TipoCobertura).WithMany(t => t.RecursoZonaCoberturas).HasForeignKey(x => x.IdTipoCobertura);
        modelBuilder.Entity<RecursoZonaBarrera>()
            .HasOne(x => x.Recurso).WithMany(r => r.RecursoZonaBarreras).HasForeignKey(x => x.IdRecurso);
        modelBuilder.Entity<RecursoZonaBarrera>()
            .HasOne(x => x.Zona).WithMany(z => z.RecursoZonaBarreras).HasForeignKey(x => x.IdZona);
        modelBuilder.Entity<RecursoZonaBarrera>()
            .HasOne(x => x.Barrera).WithMany(b => b.RecursoZonaBarreras).HasForeignKey(x => x.IdBarrera);
        modelBuilder.Entity<RecursoZonaBarrera>()
            .HasOne(x => x.GrupoPoblacional).WithMany(g => g.RecursoZonaBarreras).HasForeignKey(x => x.IdGrupoPoblacional);
        modelBuilder.Entity<RecursoZonaAccesibilidad>()
            .HasOne(x => x.Recurso).WithMany(r => r.RecursoZonaAccesibilidades).HasForeignKey(x => x.IdRecurso);
        modelBuilder.Entity<RecursoZonaAccesibilidad>()
            .HasOne(x => x.Zona).WithMany(z => z.RecursoZonaAccesibilidades).HasForeignKey(x => x.IdZona);

        // ---------- relaciones: evidencias ----------
        modelBuilder.Entity<Evidencia>()
            .HasOne(x => x.ObservacionProblematica).WithMany(o => o.Evidencias).HasForeignKey(x => x.IdObservacionProblematica);
        modelBuilder.Entity<Evidencia>()
            .HasOne(x => x.TipoEvidencia).WithMany(t => t.Evidencias).HasForeignKey(x => x.IdTipoEvidencia);
        modelBuilder.Entity<Evidencia>()
            .HasOne(x => x.Fuente).WithMany(f => f.Evidencias).HasForeignKey(x => x.IdFuente);

        // ---------- relaciones: intervención ----------
        modelBuilder.Entity<NecesidadTipoIntervencion>()
            .HasOne(x => x.Necesidad).WithMany(n => n.NecesidadTiposIntervencion).HasForeignKey(x => x.IdNecesidad);
        modelBuilder.Entity<NecesidadTipoIntervencion>()
            .HasOne(x => x.TipoIntervencion).WithMany(t => t.NecesidadTiposIntervencion).HasForeignKey(x => x.IdTipoIntervencion);
        modelBuilder.Entity<TipoIntervencionServicio>()
            .HasOne(x => x.TipoIntervencion).WithMany(t => t.TipoIntervencionServicios).HasForeignKey(x => x.IdTipoIntervencion);
        modelBuilder.Entity<TipoIntervencionServicio>()
            .HasOne(x => x.Servicio).WithMany(s => s.TipoIntervencionServicios).HasForeignKey(x => x.IdServicio);

        // ---------- relaciones: priorización y brecha ----------
        modelBuilder.Entity<ModeloPonderacion>()
            .HasOne(x => x.Modelo).WithMany(m => m.ModeloPonderaciones).HasForeignKey(x => x.IdModelo);
        modelBuilder.Entity<PrioridadCalculada>()
            .HasOne(x => x.Zona).WithMany(z => z.PrioridadesCalculada).HasForeignKey(x => x.IdZona);
        modelBuilder.Entity<PrioridadCalculada>()
            .HasOne(x => x.Problematica).WithMany(p => p.PrioridadesCalculada).HasForeignKey(x => x.IdProblematica);
        modelBuilder.Entity<PrioridadCalculada>()
            .HasOne(x => x.Periodo).WithMany(p => p.PrioridadesCalculada).HasForeignKey(x => x.IdPeriodo);
        modelBuilder.Entity<PrioridadCalculada>()
            .HasOne(x => x.GrupoPoblacional).WithMany(g => g.PrioridadesCalculada).HasForeignKey(x => x.IdGrupoPoblacional);
        modelBuilder.Entity<PrioridadCalculada>()
            .HasOne(x => x.Modelo).WithMany(m => m.PrioridadesCalculada).HasForeignKey(x => x.IdModelo);
        modelBuilder.Entity<BrechaAtencion>()
            .HasOne(x => x.Zona).WithMany(z => z.BrechasAtencion).HasForeignKey(x => x.IdZona);
        modelBuilder.Entity<BrechaAtencion>()
            .HasOne(x => x.Necesidad).WithMany(n => n.BrechasAtencion).HasForeignKey(x => x.IdNecesidad);
        modelBuilder.Entity<BrechaAtencion>()
            .HasOne(x => x.Periodo).WithMany(p => p.BrechasAtencion).HasForeignKey(x => x.IdPeriodo);

        // ---------- relaciones: módulo registros OCN ----------
        modelBuilder.Entity<RegistroOcn>()
            .HasOne(x => x.Zona).WithMany(z => z.RegistrosOcn).HasForeignKey(x => x.IdZona);
        modelBuilder.Entity<RegistroOcn>()
            .HasOne(x => x.Problematica).WithMany(p => p.RegistrosOcn).HasForeignKey(x => x.IdProblematica);
        modelBuilder.Entity<RegistroOcn>()
            .Property(x => x.Activo)
            .HasDefaultValue(true);

        // ---------- relaciones: auditoría ----------
        modelBuilder.Entity<Auditoria>()
            .HasOne(x => x.Usuario).WithMany(u => u.Auditorias).HasForeignKey(x => x.IdUsuario);

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

        // ---------- comportamiento de borrado ----------
        // La base ya existe con sus propias FKs; SQL Server rechaza múltiples rutas
        // de borrado en cascada, así que todas las FKs quedan en Restrict (NoAction),
        // igual que lo genera el scaffolding de una base database-first.
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        base.OnModelCreating(modelBuilder);
    }
}
