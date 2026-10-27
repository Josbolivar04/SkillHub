using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Repositories;
using SkillHub.Infrastructure.Data;

namespace SkillHub.Infrastructure.Repositories;

public class WorkshopRepository : IWorkshopRepository
{
    private readonly ApplicationDbContext _context;

    public WorkshopRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Workshop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workshops
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workshop?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
                .ThenInclude(e => e.Participant)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.Workshops
            .Include(w => w.Instructor)
            .FirstOrDefaultAsync(w => w.Code == normalizedCode, cancellationToken);
    }

    public async Task<IReadOnlyList<Workshop>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .OrderBy(w => w.StartDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Workshop>> GetUpcomingAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .Where(w => w.StartDateTime > now)
            .OrderBy(w => w.StartDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsWithCodeAsync(string code, Guid? excludeWorkshopId = null, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        var query = _context.Workshops.AsQueryable();

        if (excludeWorkshopId.HasValue)
        {
            query = query.Where(w => w.Id != excludeWorkshopId.Value);
        }

        return await query.AnyAsync(w => w.Code == normalizedCode, cancellationToken);
    }

    public async Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default)
    {
        await _context.Workshops.AddAsync(workshop, cancellationToken);
    }

    public void Update(Workshop workshop)
    {
        _context.Workshops.Update(workshop);
    }

    public void Delete(Workshop workshop)
    {
        _context.Workshops.Remove(workshop);
    }
}
