using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Workshops;

public record CreateWorkshopDto(
    [Required(ErrorMessage = "Code is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Code must be between 3 and 50 characters.")]
    string Code,

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    string Title,

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    string? Description,

    [Required(ErrorMessage = "Instructor email is required.")]
    [EmailAddress(ErrorMessage = "Invalid instructor email format.")]
    string InstructorEmail,

    [Required(ErrorMessage = "StartDateTime is required.")]
    DateTime StartDateTime,

    [Required(ErrorMessage = "EndDateTime is required.")]
    DateTime EndDateTime,

    [Required(ErrorMessage = "Capacity is required.")]
    [Range(1, 10000, ErrorMessage = "Capacity must be greater than 0.")]
    int Capacity
);
