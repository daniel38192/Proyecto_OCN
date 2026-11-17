namespace OCN_Project.Models.Entities;

/// <summary>Escala de magnitud (rn-68 a rn-81).</summary>
public class CatMagnitud
{
    public int IdMagnitud { get; set; }
    public string Nombre { get; set; } = null!;
    public byte? Orden { get; set; }
    public bool Activo { get; set; }

    public ICollection<ObservacionProblematica> ObservacionesProblematica { get; set; } = new List<ObservacionProblematica>();
}

/// <summary>Escala de gravedad (rn-68 a rn-81).</summary>
public class CatGravedad
{
    public int IdGravedad { get; set; }
    public string Nombre { get; set; } = null!;
    public byte? Orden { get; set; }
    public bool Activo { get; set; }

    public ICollection<ObservacionProblematica> ObservacionesProblematica { get; set; } = new List<ObservacionProblematica>();
}

/// <summary>Escala de frecuencia (rn-76, rn-78).</summary>
public class CatFrecuencia
{
    public int IdFrecuencia { get; set; }
    public string Nombre { get; set; } = null!;
    public byte? Orden { get; set; }
    public bool Activo { get; set; }

    public ICollection<ObservacionProblematica> ObservacionesProblematica { get; set; } = new List<ObservacionProblematica>();
}

/// <summary>Escala de tendencia (rn-79 a rn-81).</summary>
public class CatTendencia
{
    public int IdTendencia { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<TendenciaProblematica> TendenciasProblematica { get; set; } = new List<TendenciaProblematica>();
}

/// <summary>Hecho central: observación x problemática con escalas (rn-41/42, rn-71, rn-73-78).</summary>
public class ObservacionProblematica
{
    public int IdObservacionProblematica { get; set; }
    public int IdObservacion { get; set; }
    public int IdProblematica { get; set; }
    public int IdMagnitud { get; set; }
    public int IdGravedad { get; set; }
    public int IdFrecuencia { get; set; }
    public decimal? NivelConfianza { get; set; }

    public Observacion Observacion { get; set; } = null!;
    public Problematica Problematica { get; set; } = null!;
    public CatMagnitud Magnitud { get; set; } = null!;
    public CatGravedad Gravedad { get; set; } = null!;
    public CatFrecuencia Frecuencia { get; set; } = null!;

    public ICollection<Evidencia> Evidencias { get; set; } = new List<Evidencia>();
}

/// <summary>Tendencia calculada: zona + problemática + periodo (rn-79 a rn-81, rn-205, rn-209).</summary>
public class TendenciaProblematica
{
    public int IdTendenciaProblematica { get; set; }
    public int IdZona { get; set; }
    public int IdProblematica { get; set; }
    public int IdPeriodo { get; set; }
    public int IdTendencia { get; set; }
    public DateTime FechaCalculo { get; set; }

    public Zona Zona { get; set; } = null!;
    public Problematica Problematica { get; set; } = null!;
    public Periodo Periodo { get; set; } = null!;
    public CatTendencia Tendencia { get; set; } = null!;
}
