using SkillHub.Domain.Entities;

namespace SkillHub.Domain.Repositories;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetByWorkshopIdAsync(Guid workshopId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetActiveByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default);
    Task<Enrollment?> GetActiveEnrollmentAsync(Guid participantId, Guid workshopId, CancellationToken cancellationToken = default);
    Task<int> GetActiveEnrollmentCountAsync(Guid workshopId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveEnrollmentAsync(Guid participantId, Guid workshopId, CancellationToken cancellationToken = default);
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    void Update(Enrollment enrollment);
    void Delete(Enrollment enrollment);
}
