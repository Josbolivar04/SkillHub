using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Repositories;
using SkillHub.Infrastructure.Data;

namespace SkillHub.Infrastructure.Repositories;

public class ParticipantRepository : IParticipantRepository
{
    private readonly ApplicationDbContext _context;

    public ParticipantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Participant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Participants
            .Include(p => p.Enrollments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Participant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Participants
            .FirstOrDefaultAsync(p => p.Email == normalizedEmail, cancellationToken);
    }

    public async Task<Participant?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Participants
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Participant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Participants
            .OrderBy(p => p.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Participant participant, CancellationToken cancellationToken = default)
    {
        await _context.Participants.AddAsync(participant, cancellationToken);
    }

    public void Update(Participant participant)
    {
        _context.Participants.Update(participant);
    }

    public void Delete(Participant participant)
    {
        _context.Participants.Remove(participant);
    }
}
