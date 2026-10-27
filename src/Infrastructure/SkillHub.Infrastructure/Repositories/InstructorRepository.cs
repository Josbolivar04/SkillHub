using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Repositories;
using SkillHub.Infrastructure.Data;

namespace SkillHub.Infrastructure.Repositories;

public class InstructorRepository : IInstructorRepository
{
    private readonly ApplicationDbContext _context;

    public InstructorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Instructor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Instructors
            .Include(i => i.Workshops)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Instructor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Instructors
            .FirstOrDefaultAsync(i => i.Email == normalizedEmail, cancellationToken);
    }

    public async Task<Instructor?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Instructors
            .FirstOrDefaultAsync(i => i.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Instructor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Instructors
            .OrderBy(i => i.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Instructor instructor, CancellationToken cancellationToken = default)
    {
        await _context.Instructors.AddAsync(instructor, cancellationToken);
    }

    public void Update(Instructor instructor)
    {
        _context.Instructors.Update(instructor);
    }

    public void Delete(Instructor instructor)
    {
        _context.Instructors.Remove(instructor);
    }
}
