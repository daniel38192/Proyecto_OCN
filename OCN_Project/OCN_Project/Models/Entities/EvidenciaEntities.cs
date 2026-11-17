namespace OCN_Project.Models.Entities;

/// <summary>Catálogo de tipos de evidencia (rn-118 a rn-125).</summary>
public class CatTipoEvidencia
{
    public int IdTipoEvidencia { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<Evidencia> Evidencias { get; set; } = new List<Evidencia>();
}

/// <summary>Evidencia que respalda un hecho observación-problemática (rn-118 a rn-125).</summary>
public class Evidencia
{
    public int IdEvidencia { get; set; }
    public int IdObservacionProblematica { get; set; }
    public int IdTipoEvidencia { get; set; }
    public int? IdFuente { get; set; }
    public int NumeroMenciones { get; set; }
    public string? DescripcionBreve { get; set; }
    public DateTime FechaRegistro { get; set; }

    public ObservacionProblematica ObservacionProblematica { get; set; } = null!;
    public CatTipoEvidencia TipoEvidencia { get; set; } = null!;
    public Fuente? Fuente { get; set; }
}
