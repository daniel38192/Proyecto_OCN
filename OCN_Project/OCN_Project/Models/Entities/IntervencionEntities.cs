namespace OCN_Project.Models.Entities;

/// <summary>Catálogo de tipos de intervención (rn-139 a rn-145).</summary>
public class CatTipoIntervencion
{
    public int IdTipoIntervencion { get; set; }
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; }

    public ICollection<NecesidadTipoIntervencion> NecesidadTiposIntervencion { get; set; } = new List<NecesidadTipoIntervencion>();
    public ICollection<TipoIntervencionServicio> TipoIntervencionServicios { get; set; } = new List<TipoIntervencionServicio>();
}

/// <summary>Tabla puente necesidad-tipo de intervención (rn-139-141: n:m).</summary>
public class NecesidadTipoIntervencion
{
    public int IdNecesidad { get; set; }
    public int IdTipoIntervencion { get; set; }

    public Necesidad Necesidad { get; set; } = null!;
    public CatTipoIntervencion TipoIntervencion { get; set; } = null!;
}

/// <summary>Tabla puente tipo de intervención-servicio (rn-142-143: n:m).</summary>
public class TipoIntervencionServicio
{
    public int IdTipoIntervencion { get; set; }
    public int IdServicio { get; set; }

    public CatTipoIntervencion TipoIntervencion { get; set; } = null!;
    public CatServicio Servicio { get; set; } = null!;
}
