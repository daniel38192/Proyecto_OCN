namespace OCN_Project.Models.Entities;

/// <summary>Categoría de problemática (rn-54 a rn-60).</summary>
public class CategoriaProblematica
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<Problematica> Problematicas { get; set; } = new List<Problematica>();
}

/// <summary>Problemática jerárquica (rn-54, rn-56: subproblemáticas).</summary>
public class Problematica
{
    public int IdProblematica { get; set; }
    public int IdCategoria { get; set; }
    public int? IdProblematicaPadre { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }

    public CategoriaProblematica Categoria { get; set; } = null!;
    public Problematica? ProblematicaPadre { get; set; }
    public ICollection<Problematica> ProblematicasHijas { get; set; } = new List<Problematica>();

    public ICollection<ObservacionProblematica> ObservacionesProblematica { get; set; } = new List<ObservacionProblematica>();
    public ICollection<TendenciaProblematica> TendenciasProblematica { get; set; } = new List<TendenciaProblematica>();
    public ICollection<ProblematicaNecesidad> ProblematicaNecesidades { get; set; } = new List<ProblematicaNecesidad>();
    public ICollection<ProblematicaFactorRiesgo> ProblematicaFactoresRiesgo { get; set; } = new List<ProblematicaFactorRiesgo>();
    public ICollection<ProblematicaFactorProtector> ProblematicaFactoresProtector { get; set; } = new List<ProblematicaFactorProtector>();
    public ICollection<PrioridadCalculada> PrioridadesCalculada { get; set; } = new List<PrioridadCalculada>();
}
