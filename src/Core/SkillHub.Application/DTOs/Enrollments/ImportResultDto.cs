namespace SkillHub.Application.DTOs.Enrollments;

public record ImportErrorDto(
    int RowNumber,
    string? ParticipantEmail,
    string? WorkshopCode,
    string Reason
);

public record ImportResultDto(
    int TotalRows,
    int ImportedCount,
    int RejectedCount,
    IReadOnlyList<ImportErrorDto> Errors
);
