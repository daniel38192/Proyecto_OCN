namespace OCN_Project.Models.Entities;

/// <summary>Catálogo de roles del sistema (rn-168 a rn-175).</summary>
public class Rol
{
    public int IdRol { get; set; }
    public string NombreRol { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}

/// <summary>Personal autorizado del proyecto (rn-26), nunca integrantes de la comunidad.</summary>
public class Usuario
{
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string? CorreoInstitucional { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaAlta { get; set; }

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public ICollection<LevantamientoResponsable> LevantamientoResponsables { get; set; } = new List<LevantamientoResponsable>();
    public ICollection<Auditoria> Auditorias { get; set; } = new List<Auditoria>();
}

/// <summary>Tabla puente usuario-rol (rn-169: cada usuario pertenece al menos a un rol).</summary>
public class UsuarioRol
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
}
