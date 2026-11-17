namespace OCN_Project.Models.Entities;

/// <summary>Periodo de estudio (rn-11 a rn-16).</summary>
public class Periodo
{
    public int IdPeriodo { get; set; }
    public string CodigoPeriodo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool Activo { get; set; }

    public ICollection<Levantamiento> Levantamientos { get; set; } = new List<Levantamiento>();
    public ICollection<TendenciaProblematica> TendenciasProblematica { get; set; } = new List<TendenciaProblematica>();
    public ICollection<PrioridadCalculada> PrioridadesCalculada { get; set; } = new List<PrioridadCalculada>();
    public ICollection<BrechaAtencion> BrechasAtencion { get; set; } = new List<BrechaAtencion>();
}
