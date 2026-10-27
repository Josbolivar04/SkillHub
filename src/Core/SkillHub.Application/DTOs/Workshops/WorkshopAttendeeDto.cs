namespace SkillHub.Application.DTOs.Workshops;

public record WorkshopAttendeeDto(
    Guid EnrollmentId,
    Guid ParticipantId,
    string ParticipantName,
    string ParticipantEmail,
    DateTime EnrolledAt,
    string Status
);
