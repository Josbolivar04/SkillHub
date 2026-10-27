using System.ComponentModel.DataAnnotations;

namespace SkillHub.Web.Models;

/// <summary>
/// Form model shared by the instructor "Create workshop" and "Edit workshop" screens.
/// </summary>
public class WorkshopFormViewModel : IValidatableObject
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El código debe tener entre 3 y 50 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "Usa solo letras, números y guiones (ej. FOT-101).")]
    [Display(Name = "Código")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    [Display(Name = "Título")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La descripción no puede superar los 2000 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Inicio")]
    public DateTime StartDateTime { get; set; } = DateTime.Today.AddDays(7).AddHours(9);

    [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Finalización")]
    public DateTime EndDateTime { get; set; } = DateTime.Today.AddDays(7).AddHours(12);

    [Required(ErrorMessage = "La capacidad es obligatoria.")]
    [Range(1, 10000, ErrorMessage = "La capacidad debe ser mayor que 0.")]
    [Display(Name = "Capacidad")]
    public int Capacity { get; set; } = 15;

    /// <summary>Read-only info shown on the edit screen.</summary>
    public int ActiveEnrollments { get; set; }

    public bool IsEdit => Id.HasValue;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDateTime <= StartDateTime)
        {
            yield return new ValidationResult(
                "La fecha de finalización debe ser posterior a la de inicio.",
                new[] { nameof(EndDateTime) });
        }

        if (IsEdit && Capacity < ActiveEnrollments)
        {
            yield return new ValidationResult(
                $"La capacidad no puede ser menor que los inscritos activos ({ActiveEnrollments}).",
                new[] { nameof(Capacity) });
        }
    }
}
