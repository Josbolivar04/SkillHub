using SkillHub.Application.DTOs.Reports;
using SkillHub.Application.DTOs.Workshops;

namespace SkillHub.Application.Interfaces;

public interface IWorkshopService
{
    Task<IReadOnlyList<WorkshopDto>> GetFutureWorkshopsAsync(CancellationToken cancellationToken = default);
    Task<WorkshopDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkshopDto> CreateWorkshopAsync(CreateWorkshopDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkshopAttendeeDto>> GetWorkshopAttendeesAsync(Guid workshopId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TopWorkshopReportDto>> GetTopWorkshopsReportAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every workshop (past and upcoming) assigned to the instructor with the given email.
    /// </summary>
    Task<IReadOnlyList<WorkshopDto>> GetWorkshopsByInstructorAsync(string instructorEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a workshop. Only the instructor assigned to the workshop is allowed to edit it.
    /// </summary>
    Task<WorkshopDto> UpdateWorkshopAsync(Guid workshopId, UpdateWorkshopDto request, string instructorEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true when the workshop is assigned to the instructor with the given email.
    /// </summary>
    Task<bool> IsWorkshopOwnedByInstructorAsync(Guid workshopId, string instructorEmail, CancellationToken cancellationToken = default);
}
