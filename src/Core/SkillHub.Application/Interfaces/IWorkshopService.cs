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
}
