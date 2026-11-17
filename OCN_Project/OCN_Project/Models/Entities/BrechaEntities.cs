namespace OCN_Project.Models.Entities;

/// <summary>Indicador derivado de brecha de atención (rn-133 a rn-138).</summary>
public class BrechaAtencion
{
    public int IdBrechaAtencion { get; set; }
    public int IdZona { get; set; }
    public int IdNecesidad { get; set; }
    public int IdPeriodo { get; set; }
    public int RecursosIdentificados { get; set; }
    public string NivelBrecha { get; set; } = null!;
    public DateTime FechaCalculo { get; set; }

    public Zona Zona { get; set; } = null!;
    public Necesidad Necesidad { get; set; } = null!;
    public Periodo Periodo { get; set; } = null!;
}
