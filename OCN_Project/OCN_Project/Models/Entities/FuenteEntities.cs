namespace OCN_Project.Models.Entities;

/// <summary>Catálogo de tipos de fuente (rn-33 a rn-38).</summary>
public class CatTipoFuente
{
    public int IdTipoFuente { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<Fuente> Fuentes { get; set; } = new List<Fuente>();
}

/// <summary>Catálogo de tipo de organización (rn-149): gubernamental, educativa, sanitaria, religiosa...</summary>
public class CatTipoOrganizacion
{
    public int IdTipoOrganizacion { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<Organizacion> Organizaciones { get; set; } = new List<Organizacion>();
}

/// <summary>Organización (rn-146 a rn-150).</summary>
public class Organizacion
{
    public int IdOrganizacion { get; set; }
    public string Nombre { get; set; } = null!;
    public int IdTipoOrganizacion { get; set; }
    public bool Activo { get; set; }

    public CatTipoOrganizacion TipoOrganizacion { get; set; } = null!;

    public ICollection<Fuente> Fuentes { get; set; } = new List<Fuente>();
    public ICollection<Recurso> Recursos { get; set; } = new List<Recurso>();
}

/// <summary>Fuente de información (rn-37): distingue tipo de fuente vs. organización concreta.</summary>
public class Fuente
{
    public int IdFuente { get; set; }
    public int IdTipoFuente { get; set; }
    public int? IdOrganizacion { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }

    public CatTipoFuente TipoFuente { get; set; } = null!;
    public Organizacion? Organizacion { get; set; }

    public ICollection<LevantamientoFuente> LevantamientoFuentes { get; set; } = new List<LevantamientoFuente>();
    public ICollection<ObservacionFuente> ObservacionFuentes { get; set; } = new List<ObservacionFuente>();
    public ICollection<Evidencia> Evidencias { get; set; } = new List<Evidencia>();
}
