namespace OCN_Project.Models.Entities;

/// <summary>Catálogo de tipos de cobertura (rn-98-100, rn-109-117).</summary>
public class CatTipoCobertura
{
    public int IdTipoCobertura { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<RecursoZonaCobertura> RecursoZonaCoberturas { get; set; } = new List<RecursoZonaCobertura>();
}

/// <summary>Cobertura histórica recurso-zona (rn-98-100, rn-109-112; fecha fin null = vigente).</summary>
public class RecursoZonaCobertura
{
    public int IdRecursoZonaCobertura { get; set; }
    public int IdRecurso { get; set; }
    public int IdZona { get; set; }
    public int IdTipoCobertura { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    public Recurso Recurso { get; set; } = null!;
    public Zona Zona { get; set; } = null!;
    public CatTipoCobertura TipoCobertura { get; set; } = null!;
}

/// <summary>Catálogo de barreras de acceso (rn-113-116).</summary>
public class CatBarrera
{
    public int IdBarrera { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<RecursoZonaBarrera> RecursoZonaBarreras { get; set; } = new List<RecursoZonaBarrera>();
}

/// <summary>Barrera por recurso + zona + grupo poblacional (rn-113-116).</summary>
public class RecursoZonaBarrera
{
    public int IdRecursoZonaBarrera { get; set; }
    public int IdRecurso { get; set; }
    public int IdZona { get; set; }
    public int IdBarrera { get; set; }
    public int? IdGrupoPoblacional { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string? Notas { get; set; }

    public Recurso Recurso { get; set; } = null!;
    public Zona Zona { get; set; } = null!;
    public CatBarrera Barrera { get; set; } = null!;
    public CatGrupoPoblacional? GrupoPoblacional { get; set; }
}

/// <summary>Accesibilidad percibida por recurso + zona (rn-117).</summary>
public class RecursoZonaAccesibilidad
{
    public int IdRecursoZonaAccesibilidad { get; set; }
    public int IdRecurso { get; set; }
    public int IdZona { get; set; }
    public string NivelAccesibilidadPercibida { get; set; } = null!;
    public DateTime FechaEvaluacion { get; set; }

    public Recurso Recurso { get; set; } = null!;
    public Zona Zona { get; set; } = null!;
}
