namespace SkillHub.Application.DTOs.Workshops;

public record WorkshopDto(
    Guid Id,
    string Code,
    string Title,
    string Description,
    Guid InstructorId,
    string? InstructorName,
    string? InstructorEmail,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int Capacity,
    int ActiveEnrollments,
    int AvailableSlots
);
