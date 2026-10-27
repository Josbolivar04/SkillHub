using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Workshops;

/// <summary>
/// Editable fields of an existing workshop. The workshop code and assigned instructor are immutable.
/// </summary>
public record UpdateWorkshopDto(
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    string Title,

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    string? Description,

    [Required(ErrorMessage = "StartDateTime is required.")]
    DateTime StartDateTime,

    [Required(ErrorMessage = "EndDateTime is required.")]
    DateTime EndDateTime,

    [Required(ErrorMessage = "Capacity is required.")]
    [Range(1, 10000, ErrorMessage = "Capacity must be greater than 0.")]
    int Capacity
);
