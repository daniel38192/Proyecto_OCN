namespace OCN_Project.Models.Entities;

/// <summary>Colonia / división territorial principal (rn-04, rn-05).</summary>
public class DivisionTerritorial
{
    public int IdDivision { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Tipo { get; set; }
    public bool Activo { get; set; }

    public ICollection<Zona> Zonas { get; set; } = new List<Zona>();
}

/// <summary>Catálogo de tipo de zona: urbana, periurbana, rural (rn-06).</summary>
public class CatTipoZona
{
    public int IdTipoZona { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<Zona> Zonas { get; set; } = new List<Zona>();
}

/// <summary>Zona de estudio (rn-01 a rn-10).</summary>
public class Zona
{
    public int IdZona { get; set; }
    public string CodigoZona { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public int IdDivision { get; set; }
    public int IdTipoZona { get; set; }
    public string? DescripcionGeneral { get; set; }
    public string? ReferenciaGeograficaAprox { get; set; }
    public bool Activo { get; set; }

    public DivisionTerritorial Division { get; set; } = null!;
    public CatTipoZona TipoZona { get; set; } = null!;

    public ICollection<Levantamiento> Levantamientos { get; set; } = new List<Levantamiento>();
    public ICollection<TendenciaProblematica> TendenciasProblematica { get; set; } = new List<TendenciaProblematica>();
    public ICollection<RecursoZonaCobertura> RecursoZonaCoberturas { get; set; } = new List<RecursoZonaCobertura>();
    public ICollection<RecursoZonaBarrera> RecursoZonaBarreras { get; set; } = new List<RecursoZonaBarrera>();
    public ICollection<RecursoZonaAccesibilidad> RecursoZonaAccesibilidades { get; set; } = new List<RecursoZonaAccesibilidad>();
    public ICollection<PrioridadCalculada> PrioridadesCalculada { get; set; } = new List<PrioridadCalculada>();
    public ICollection<BrechaAtencion> BrechasAtencion { get; set; } = new List<BrechaAtencion>();
}
