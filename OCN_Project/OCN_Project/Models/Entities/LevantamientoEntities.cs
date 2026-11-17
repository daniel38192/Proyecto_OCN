namespace OCN_Project.Models.Entities;

/// <summary>Levantamiento en una zona, durante un periodo, con un método (rn-17 a rn-27).</summary>
public class Levantamiento
{
    public int IdLevantamiento { get; set; }
    public string CodigoLevantamiento { get; set; } = null!;
    public int IdZona { get; set; }
    public int IdPeriodo { get; set; }
    public int IdMetodo { get; set; }
    public DateTime FechaLevantamiento { get; set; }
    public int? NumeroParticipantesAprox { get; set; }
    public string? ObservacionesMetodologicas { get; set; }
    public DateTime FechaCaptura { get; set; }

    public Zona Zona { get; set; } = null!;
    public Periodo Periodo { get; set; } = null!;
    public CatMetodo Metodo { get; set; } = null!;

    public ICollection<LevantamientoInstrumento> LevantamientoInstrumentos { get; set; } = new List<LevantamientoInstrumento>();
    public ICollection<LevantamientoResponsable> LevantamientoResponsables { get; set; } = new List<LevantamientoResponsable>();
    public ICollection<LevantamientoFuente> LevantamientoFuentes { get; set; } = new List<LevantamientoFuente>();
    public ICollection<Observacion> Observaciones { get; set; } = new List<Observacion>();
}

/// <summary>Tabla puente levantamiento-instrumento (rn-19, rn-20: n:m).</summary>
public class LevantamientoInstrumento
{
    public int IdLevantamiento { get; set; }
    public int IdInstrumento { get; set; }

    public Levantamiento Levantamiento { get; set; } = null!;
    public Instrumento Instrumento { get; set; } = null!;
}

/// <summary>Tabla puente levantamiento-responsable (rn-25, rn-26: equipo institucional).</summary>
public class LevantamientoResponsable
{
    public int IdLevantamiento { get; set; }
    public int IdUsuario { get; set; }
    public string? RolEnLevantamiento { get; set; }

    public Levantamiento Levantamiento { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}

/// <summary>Tabla puente levantamiento-fuente (rn-23: n:m).</summary>
public class LevantamientoFuente
{
    public int IdLevantamiento { get; set; }
    public int IdFuente { get; set; }

    public Levantamiento Levantamiento { get; set; } = null!;
    public Fuente Fuente { get; set; } = null!;
}
