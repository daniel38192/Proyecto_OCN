using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OCN_Project.Models.Entities;

/// <summary>
/// Registro de detección OCN (módulo principal de la WebApp).
/// FKs hacia Zona y Problematica; campos numéricos Intensidad y Frecuencia
/// validados en rango 1-10 con Data Annotations.
/// </summary>
[Table("registroocn", Schema = "ocn")]
public class RegistroOcn
{
    [Key]
    public int IdRegistroOcn { get; set; }

    [Required(ErrorMessage = "La zona es obligatoria.")]
    [Display(Name = "Zona")]
    public int IdZona { get; set; }

    [Required(ErrorMessage = "La problemática es obligatoria.")]
    [Display(Name = "Problemática")]
    public int IdProblematica { get; set; }

    [Required(ErrorMessage = "La intensidad es obligatoria.")]
    [Range(1, 10, ErrorMessage = "La intensidad debe estar entre 1 y 10.")]
    [Display(Name = "Intensidad (1-10)")]
    public int Intensidad { get; set; }

    [Required(ErrorMessage = "La frecuencia es obligatoria.")]
    [Range(1, 10, ErrorMessage = "La frecuencia debe estar entre 1 y 10.")]
    [Display(Name = "Frecuencia (1-10)")]
    public int Frecuencia { get; set; }

    [StringLength(500, ErrorMessage = "La nota no puede exceder 500 caracteres.")]
    [Display(Name = "Nota de contexto")]
    public string? NotaContexto { get; set; }

    [Display(Name = "Fecha de registro")]
    [DataType(DataType.DateTime)]
    public DateTime FechaRegistro { get; set; }

    [ForeignKey(nameof(IdZona))]
    public Zona? Zona { get; set; }

    [ForeignKey(nameof(IdProblematica))]
    public Problematica? Problematica { get; set; }
}
