namespace OCN_Project.Models.Entities;

/// <summary>Modelo de priorización comunitaria (rn-126 a rn-132).</summary>
public class ModeloPriorizacion
{
    public int IdModelo { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }

    public ICollection<ModeloPonderacion> ModeloPonderaciones { get; set; } = new List<ModeloPonderacion>();
    public ICollection<PrioridadCalculada> PrioridadesCalculada { get; set; } = new List<PrioridadCalculada>();
}

/// <summary>Pesos por variable, por modelo (rn-128, rn-130).</summary>
public class ModeloPonderacion
{
    public int IdModelo { get; set; }
    public string Variable { get; set; } = null!;
    public decimal Peso { get; set; }

    public ModeloPriorizacion Modelo { get; set; } = null!;
}

/// <summary>Prioridad calculada por zona + problemática + periodo (+ grupo) (rn-127, rn-129).</summary>
public class PrioridadCalculada
{
    public int IdPrioridadCalculada { get; set; }
    public int IdZona { get; set; }
    public int IdProblematica { get; set; }
    public int IdPeriodo { get; set; }
    public int? IdGrupoPoblacional { get; set; }
    public int IdModelo { get; set; }
    public decimal? ValorMagnitud { get; set; }
    public decimal? ValorGravedad { get; set; }
    public decimal? ValorFrecuencia { get; set; }
    public decimal? ValorVulnerabilidad { get; set; }
    public decimal? ValorEvidencia { get; set; }
    public decimal? ValorTendencia { get; set; }
    public decimal? ValorBrecha { get; set; }
    public decimal PuntajeResultante { get; set; }
    public DateTime FechaCalculo { get; set; }

    public Zona Zona { get; set; } = null!;
    public Problematica Problematica { get; set; } = null!;
    public Periodo Periodo { get; set; } = null!;
    public CatGrupoPoblacional? GrupoPoblacional { get; set; }
    public ModeloPriorizacion Modelo { get; set; } = null!;
}
