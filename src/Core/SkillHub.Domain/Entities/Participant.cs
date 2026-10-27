using SkillHub.Domain.Common;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Participant : Entity
{
    private readonly List<Enrollment> _enrollments = new();

    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? UserId { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    public virtual IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

    // Required by EF Core
    protected Participant() : base()
    {
    }

    public Participant(Guid id, string fullName, string email, string? userId = null, DateTime? registeredAt = null)
        : base(id)
    {
        ValidateAndSet(fullName, email);
        UserId = userId;
        RegisteredAt = registeredAt ?? DateTime.UtcNow;
    }

    public static Participant Create(string fullName, string email, string? userId = null)
    {
        return new Participant(Guid.NewGuid(), fullName, email, userId, DateTime.UtcNow);
    }

    public void UpdateProfile(string fullName, string email)
    {
        ValidateAndSet(fullName, email);
    }

    public void LinkUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new BusinessRuleViolationException("UserId cannot be empty when linking to a participant.");

        UserId = userId;
    }

    private void ValidateAndSet(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleViolationException("Participant full name is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleViolationException("Participant email is required.");

        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
    }
}
