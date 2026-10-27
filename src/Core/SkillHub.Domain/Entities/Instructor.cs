using SkillHub.Domain.Common;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Instructor : Entity
{
    private readonly List<Workshop> _workshops = new();

    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Specialty { get; private set; } = string.Empty;
    public string? UserId { get; private set; }

    public virtual IReadOnlyCollection<Workshop> Workshops => _workshops.AsReadOnly();

    // Required by EF Core
    protected Instructor() : base()
    {
    }

    public Instructor(Guid id, string fullName, string email, string specialty, string? userId = null)
        : base(id)
    {
        ValidateAndSet(fullName, email, specialty);
        UserId = userId;
    }

    public static Instructor Create(string fullName, string email, string specialty, string? userId = null)
    {
        return new Instructor(Guid.NewGuid(), fullName, email, specialty, userId);
    }

    public void UpdateProfile(string fullName, string email, string specialty)
    {
        ValidateAndSet(fullName, email, specialty);
    }

    public void LinkUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new BusinessRuleViolationException("UserId cannot be empty when linking to an instructor.");

        UserId = userId;
    }

    private void ValidateAndSet(string fullName, string email, string specialty)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleViolationException("Instructor full name is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleViolationException("Instructor email is required.");

        if (string.IsNullOrWhiteSpace(specialty))
            throw new BusinessRuleViolationException("Instructor specialty is required.");

        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Specialty = specialty.Trim();
    }
}
