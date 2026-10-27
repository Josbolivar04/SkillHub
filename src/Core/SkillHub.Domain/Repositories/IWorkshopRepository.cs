using SkillHub.Domain.Entities;

namespace SkillHub.Domain.Repositories;

public interface IWorkshopRepository
{
    Task<Workshop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workshop?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workshop>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workshop>> GetUpcomingAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsWithCodeAsync(string code, Guid? excludeWorkshopId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default);
    void Update(Workshop workshop);
    void Delete(Workshop workshop);
}
