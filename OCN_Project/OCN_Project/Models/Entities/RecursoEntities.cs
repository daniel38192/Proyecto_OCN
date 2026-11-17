namespace OCN_Project.Models.Entities;

/// <summary>Catálogo de tipos de recurso (rn-92 a rn-103).</summary>
public class CatTipoRecurso
{
    public int IdTipoRecurso { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<Recurso> Recursos { get; set; } = new List<Recurso>();
}

/// <summary>Catálogo de estado de recurso: activo, temporal, suspendido, cerrado, desconocido.</summary>
public class CatEstadoRecurso
{
    public int IdEstadoRecurso { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<Recurso> Recursos { get; set; } = new List<Recurso>();
}

/// <summary>Catálogo de costo de recurso: gratuito, subsidiado, con costo.</summary>
public class CatCostoRecurso
{
    public int IdCostoRecurso { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<Recurso> Recursos { get; set; } = new List<Recurso>();
}

/// <summary>Recurso comunitario (rn-92 a rn-103). No almacena domicilios particulares.</summary>
public class Recurso
{
    public int IdRecurso { get; set; }
    public string CodigoRecurso { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public int IdTipoRecurso { get; set; }
    public int IdOrganizacionResponsable { get; set; }
    public int IdEstadoRecurso { get; set; }
    public int IdCostoRecurso { get; set; }
    public string? HorarioGeneral { get; set; }
    public int? CapacidadAproximada { get; set; }
    public string? ConocimientoComunidad { get; set; }
    public bool Activo { get; set; }

    public CatTipoRecurso TipoRecurso { get; set; } = null!;
    public Organizacion OrganizacionResponsable { get; set; } = null!;
    public CatEstadoRecurso EstadoRecurso { get; set; } = null!;
    public CatCostoRecurso CostoRecurso { get; set; } = null!;

    public ICollection<RecursoServicio> RecursoServicios { get; set; } = new List<RecursoServicio>();
    public ICollection<FactorProtectorRecurso> FactorProtectorRecursos { get; set; } = new List<FactorProtectorRecurso>();
    public ICollection<RecursoZonaCobertura> RecursoZonaCoberturas { get; set; } = new List<RecursoZonaCobertura>();
    public ICollection<RecursoZonaBarrera> RecursoZonaBarreras { get; set; } = new List<RecursoZonaBarrera>();
    public ICollection<RecursoZonaAccesibilidad> RecursoZonaAccesibilidades { get; set; } = new List<RecursoZonaAccesibilidad>();
}

/// <summary>Tabla puente factor protector-recurso (rn-91: n:m).</summary>
public class FactorProtectorRecurso
{
    public int IdFactorProtector { get; set; }
    public int IdRecurso { get; set; }

    public CatFactorProtector FactorProtector { get; set; } = null!;
    public Recurso Recurso { get; set; } = null!;
}

/// <summary>Catálogo de servicios (rn-95, rn-96, rn-104 a rn-108).</summary>
public class CatServicio
{
    public int IdServicio { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<RecursoServicio> RecursoServicios { get; set; } = new List<RecursoServicio>();
    public ICollection<ServicioNecesidad> ServicioNecesidades { get; set; } = new List<ServicioNecesidad>();
    public ICollection<TipoIntervencionServicio> TipoIntervencionServicios { get; set; } = new List<TipoIntervencionServicio>();
}

/// <summary>Tabla puente recurso-servicio (rn-95, rn-96: n:m).</summary>
public class RecursoServicio
{
    public int IdRecurso { get; set; }
    public int IdServicio { get; set; }

    public Recurso Recurso { get; set; } = null!;
    public CatServicio Servicio { get; set; } = null!;
}

/// <summary>Tabla puente servicio-necesidad (rn-107, rn-108: n:m).</summary>
public class ServicioNecesidad
{
    public int IdServicio { get; set; }
    public int IdNecesidad { get; set; }

    public CatServicio Servicio { get; set; } = null!;
    public Necesidad Necesidad { get; set; } = null!;
}
