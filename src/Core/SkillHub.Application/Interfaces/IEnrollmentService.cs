using SkillHub.Application.DTOs.Enrollments;

namespace SkillHub.Application.Interfaces;

public interface IEnrollmentService : IExcelImporter
{
    Task<EnrollmentDto> EnrollAsync(string userId, CreateEnrollmentDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EnrollmentDto>> GetMyEnrollmentsAsync(string userId, CancellationToken cancellationToken = default);
    Task<EnrollmentDto> CancelEnrollmentAsync(Guid enrollmentId, string userId, bool isEmployee, CancellationToken cancellationToken = default);
}
