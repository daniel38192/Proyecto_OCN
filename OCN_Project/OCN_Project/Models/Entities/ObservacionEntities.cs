namespace OCN_Project.Models.Entities;

/// <summary>Observación comunitaria ligada a un levantamiento (rn-39 a rn-47).</summary>
public class Observacion
{
    public int IdObservacion { get; set; }
    public int IdLevantamiento { get; set; }
    public string? NotaContexto { get; set; }
    public DateTime FechaRegistro { get; set; }

    public Levantamiento Levantamiento { get; set; } = null!;

    public ICollection<ObservacionFuente> ObservacionFuentes { get; set; } = new List<ObservacionFuente>();
    public ICollection<ObservacionGrupoPoblacional> ObservacionGruposPoblacional { get; set; } = new List<ObservacionGrupoPoblacional>();
    public ICollection<ObservacionProblematica> ObservacionesProblematica { get; set; } = new List<ObservacionProblematica>();
}

/// <summary>Tabla puente observación-fuente (rn-33, rn-35, rn-36: n:m).</summary>
public class ObservacionFuente
{
    public int IdObservacion { get; set; }
    public int IdFuente { get; set; }

    public Observacion Observacion { get; set; } = null!;
    public Fuente Fuente { get; set; } = null!;
}

/// <summary>Catálogo de grupos poblacionales (rn-48 a rn-53).</summary>
public class CatGrupoPoblacional
{
    public int IdGrupoPoblacional { get; set; }
    public string Nombre { get; set; } = null!;
    public string? RangoEdad { get; set; }
    public bool Activo { get; set; }

    public ICollection<ObservacionGrupoPoblacional> ObservacionGruposPoblacional { get; set; } = new List<ObservacionGrupoPoblacional>();
    public ICollection<RecursoZonaBarrera> RecursoZonaBarreras { get; set; } = new List<RecursoZonaBarrera>();
    public ICollection<PrioridadCalculada> PrioridadesCalculada { get; set; } = new List<PrioridadCalculada>();
}

/// <summary>Tabla puente observación-grupo poblacional (rn-43, rn-49, rn-50: n:m).</summary>
public class ObservacionGrupoPoblacional
{
    public int IdObservacion { get; set; }
    public int IdGrupoPoblacional { get; set; }

    public Observacion Observacion { get; set; } = null!;
    public CatGrupoPoblacional GrupoPoblacional { get; set; } = null!;
}
