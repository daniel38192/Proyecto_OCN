namespace OCN_Project.Models.Entities;

/// <summary>Auditoría y trazabilidad (rn-174, rn-175, rn-182 a rn-187).</summary>
public class Auditoria
{
    public long IdAuditoria { get; set; }
    public string NombreTabla { get; set; } = null!;
    public int IdRegistro { get; set; }
    public string Operacion { get; set; } = null!;
    public int? IdUsuario { get; set; }
    public DateTime FechaOperacion { get; set; }
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }

    public Usuario? Usuario { get; set; }
}
