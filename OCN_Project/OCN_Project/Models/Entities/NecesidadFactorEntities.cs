namespace OCN_Project.Models.Entities;

/// <summary>Necesidad comunitaria (rn-61 a rn-67).</summary>
public class Necesidad
{
    public int IdNecesidad { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }

    public ICollection<ProblematicaNecesidad> ProblematicaNecesidades { get; set; } = new List<ProblematicaNecesidad>();
    public ICollection<ServicioNecesidad> ServicioNecesidades { get; set; } = new List<ServicioNecesidad>();
    public ICollection<NecesidadTipoIntervencion> NecesidadTiposIntervencion { get; set; } = new List<NecesidadTipoIntervencion>();
    public ICollection<BrechaAtencion> BrechasAtencion { get; set; } = new List<BrechaAtencion>();
}

/// <summary>Tabla puente problemática-necesidad (rn-62, rn-63: n:m).</summary>
public class ProblematicaNecesidad
{
    public int IdProblematica { get; set; }
    public int IdNecesidad { get; set; }

    public Problematica Problematica { get; set; } = null!;
    public Necesidad Necesidad { get; set; } = null!;
}

/// <summary>Catálogo de factores de riesgo (rn-82 a rn-91).</summary>
public class CatFactorRiesgo
{
    public int IdFactorRiesgo { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<ProblematicaFactorRiesgo> ProblematicaFactoresRiesgo { get; set; } = new List<ProblematicaFactorRiesgo>();
}

/// <summary>Tabla puente problemática-factor de riesgo (rn-83, rn-84: n:m).</summary>
public class ProblematicaFactorRiesgo
{
    public int IdProblematica { get; set; }
    public int IdFactorRiesgo { get; set; }

    public Problematica Problematica { get; set; } = null!;
    public CatFactorRiesgo FactorRiesgo { get; set; } = null!;
}

/// <summary>Catálogo de factores protectores (rn-88 a rn-91).</summary>
public class CatFactorProtector
{
    public int IdFactorProtector { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<ProblematicaFactorProtector> ProblematicaFactoresProtector { get; set; } = new List<ProblematicaFactorProtector>();
    public ICollection<FactorProtectorRecurso> FactorProtectorRecursos { get; set; } = new List<FactorProtectorRecurso>();
}

/// <summary>Tabla puente problemática-factor protector (rn-88, rn-89: n:m).</summary>
public class ProblematicaFactorProtector
{
    public int IdProblematica { get; set; }
    public int IdFactorProtector { get; set; }

    public Problematica Problematica { get; set; } = null!;
    public CatFactorProtector FactorProtector { get; set; } = null!;
}
