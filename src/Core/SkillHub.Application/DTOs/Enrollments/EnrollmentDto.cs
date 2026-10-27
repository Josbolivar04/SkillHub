namespace SkillHub.Application.DTOs.Enrollments;

public record EnrollmentDto(
    Guid Id,
    Guid ParticipantId,
    string? ParticipantName,
    string? ParticipantEmail,
    Guid WorkshopId,
    string WorkshopCode,
    string WorkshopTitle,
    DateTime WorkshopStartDateTime,
    DateTime WorkshopEndDateTime,
    string Status,
    DateTime EnrolledAt,
    DateTime? CancelledAt,
    string? CancelledBy
);
