namespace SkillHub.Application.DTOs.Reports;

public record TopWorkshopReportDto(
    Guid WorkshopId,
    string Code,
    string Title,
    string? InstructorName,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int Capacity,
    int ActiveEnrollments,
    int AvailableSlots,
    double OccupancyPercentage
);
