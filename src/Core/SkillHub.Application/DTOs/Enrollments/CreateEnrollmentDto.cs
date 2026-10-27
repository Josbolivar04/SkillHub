using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Enrollments;

public record CreateEnrollmentDto(
    [Required(ErrorMessage = "WorkshopId is required.")]
    Guid WorkshopId
);
