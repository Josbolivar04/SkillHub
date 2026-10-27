using SkillHub.Domain.Common;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Enrollment : Entity
{
    public Guid ParticipantId { get; private set; }
    public Guid WorkshopId { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancelledBy { get; private set; }

    public virtual Participant? Participant { get; private set; }
    public virtual Workshop? Workshop { get; private set; }

    // Required by EF Core
    protected Enrollment() : base()
    {
    }

    public Enrollment(
        Guid id,
        Guid participantId,
        Guid workshopId,
        EnrollmentStatus status,
        DateTime enrolledAt,
        DateTime? cancelledAt = null,
        string? cancelledBy = null)
        : base(id)
    {
        if (participantId == Guid.Empty)
            throw new BusinessRuleViolationException("ParticipantId cannot be empty.");

        if (workshopId == Guid.Empty)
            throw new BusinessRuleViolationException("WorkshopId cannot be empty.");

        ParticipantId = participantId;
        WorkshopId = workshopId;
        Status = status;
        EnrolledAt = enrolledAt;
        CancelledAt = cancelledAt;
        CancelledBy = cancelledBy;
    }

    public static Enrollment Create(Guid participantId, Guid workshopId)
    {
        return new Enrollment(
            Guid.NewGuid(),
            participantId,
            workshopId,
            EnrollmentStatus.Active,
            DateTime.UtcNow);
    }

    public void Cancel(string cancelledBy)
    {
        if (Status == EnrollmentStatus.Cancelled)
            throw new BusinessRuleViolationException("Enrollment is already cancelled.");

        if (string.IsNullOrWhiteSpace(cancelledBy))
            throw new BusinessRuleViolationException("CancelledBy must be provided when cancelling an enrollment.");

        Status = EnrollmentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancelledBy = cancelledBy.Trim();
    }
}
