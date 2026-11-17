namespace OCN_Project.Models.Entities;

/// <summary>Catálogo de métodos: encuesta, entrevista, grupo focal, observación de campo, recorrido (rn-28 a rn-32).</summary>
public class CatMetodo
{
    public int IdMetodo { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<Instrumento> Instrumentos { get; set; } = new List<Instrumento>();
    public ICollection<Levantamiento> Levantamientos { get; set; } = new List<Levantamiento>();
}

/// <summary>Instrumento con versiones (rn-31: no se sobrescriben versiones).</summary>
public class Instrumento
{
    public int IdInstrumento { get; set; }
    public string NombreFamilia { get; set; } = null!;
    public string Version { get; set; } = null!;
    public int IdMetodo { get; set; }
    public string Estado { get; set; } = null!;
    public int? IdInstrumentoAnterior { get; set; }
    public DateTime FechaVersion { get; set; }
    public bool Activo { get; set; }

    public CatMetodo Metodo { get; set; } = null!;
    public Instrumento? InstrumentoAnterior { get; set; }
    public ICollection<Instrumento> VersionesPosteriores { get; set; } = new List<Instrumento>();

    public ICollection<LevantamientoInstrumento> LevantamientoInstrumentos { get; set; } = new List<LevantamientoInstrumento>();
}
