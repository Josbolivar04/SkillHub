using SkillHub.Domain.Common;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.ValueObjects;

namespace SkillHub.Domain.Entities;

public class Workshop : Entity
{
    private readonly List<Enrollment> _enrollments = new();

    public string Code { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid InstructorId { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public int Capacity { get; private set; }

    public virtual Instructor? Instructor { get; private set; }
    public virtual IReadOnlyCollection<Enrollment> Enrollments => _enrollments.AsReadOnly();

    // Required by EF Core
    protected Workshop() : base()
    {
    }

    public Workshop(
        Guid id,
        string code,
        string title,
        string description,
        Guid instructorId,
        DateTime startDateTime,
        DateTime endDateTime,
        int capacity)
        : base(id)
    {
        ValidateScheduleInvariants(startDateTime, endDateTime, checkFuture: true);
        ValidateCapacityInvariant(capacity);
        ValidateFields(code, title, instructorId);

        Code = WorkshopCode.Create(code).Value;
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        InstructorId = instructorId;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        Capacity = capacity;
    }

    public static Workshop Create(
        string code,
        string title,
        string description,
        Guid instructorId,
        DateTime startDateTime,
        DateTime endDateTime,
        int capacity)
    {
        return new Workshop(
            Guid.NewGuid(),
            code,
            title,
            description,
            instructorId,
            startDateTime,
            endDateTime,
            capacity);
    }

    public void Reschedule(DateTime newStartDateTime, DateTime newEndDateTime)
    {
        ValidateScheduleInvariants(newStartDateTime, newEndDateTime, checkFuture: true);
        StartDateTime = newStartDateTime;
        EndDateTime = newEndDateTime;
    }

    public void UpdateDetails(string title, string description, int capacity)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleViolationException("Workshop title cannot be empty.");

        ValidateCapacityInvariant(capacity);

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Capacity = capacity;
    }

    public void AssignInstructor(Guid instructorId)
    {
        if (instructorId == Guid.Empty)
            throw new BusinessRuleViolationException("InstructorId cannot be empty.");

        InstructorId = instructorId;
    }

    public int GetActiveEnrollmentCount()
    {
        return _enrollments.Count(e => e.Status == EnrollmentStatus.Active);
    }

    public bool HasAvailableCapacity()
    {
        return GetActiveEnrollmentCount() < Capacity;
    }

    private static void ValidateScheduleInvariants(DateTime startDateTime, DateTime endDateTime, bool checkFuture)
    {
        if (checkFuture && startDateTime <= DateTime.UtcNow)
            throw new BusinessRuleViolationException("Start time must be in the future.");

        if (endDateTime <= startDateTime)
            throw new BusinessRuleViolationException("End time must be after start time.");
    }

    private static void ValidateCapacityInvariant(int capacity)
    {
        if (capacity <= 0)
            throw new BusinessRuleViolationException("Capacity must be greater than 0.");
    }

    private static void ValidateFields(string code, string title, Guid instructorId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleViolationException("Workshop code cannot be empty.");

        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleViolationException("Workshop title cannot be empty.");

        if (instructorId == Guid.Empty)
            throw new BusinessRuleViolationException("InstructorId cannot be empty.");
    }
}
