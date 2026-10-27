using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using Xunit;

namespace SkillHub.Domain.UnitTests;

public class EnrollmentTests
{
    private readonly Guid _participantId = Guid.NewGuid();
    private readonly Guid _workshopId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidIds_ShouldInitializeActiveEnrollment()
    {
        // Act
        var enrollment = Enrollment.Create(_participantId, _workshopId);

        // Assert
        Assert.NotNull(enrollment);
        Assert.NotEqual(Guid.Empty, enrollment.Id);
        Assert.Equal(_participantId, enrollment.ParticipantId);
        Assert.Equal(_workshopId, enrollment.WorkshopId);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
        Assert.True((DateTime.UtcNow - enrollment.EnrolledAt).TotalSeconds < 5);
        Assert.Null(enrollment.CancelledAt);
        Assert.Null(enrollment.CancelledBy);
    }

    [Fact]
    public void Cancel_WhenActive_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var enrollment = Enrollment.Create(_participantId, _workshopId);

        // Act
        enrollment.Cancel("admin@skillhub.com");

        // Assert
        Assert.Equal(EnrollmentStatus.Cancelled, enrollment.Status);
        Assert.NotNull(enrollment.CancelledAt);
        Assert.Equal("admin@skillhub.com", enrollment.CancelledBy);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var enrollment = Enrollment.Create(_participantId, _workshopId);
        enrollment.Cancel("admin@skillhub.com");

        // Act & Assert
        var ex = Assert.Throws<BusinessRuleViolationException>(() =>
            enrollment.Cancel("user@skillhub.com"));

        Assert.Contains("already cancelled", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_WithEmptyCancelledBy_ShouldThrowBusinessRuleViolationException(string invalidCancelledBy)
    {
        // Arrange
        var enrollment = Enrollment.Create(_participantId, _workshopId);

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            enrollment.Cancel(invalidCancelledBy));
    }
}
