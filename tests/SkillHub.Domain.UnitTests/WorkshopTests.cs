using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;
using Xunit;

namespace SkillHub.Domain.UnitTests;

public class WorkshopTests
{
    private readonly Guid _instructorId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(2);
        var end = start.AddHours(2);

        // Act
        var workshop = Workshop.Create("FOT-001", "Photography 101", "Learn basics", _instructorId, start, end, 20);

        // Assert
        Assert.NotNull(workshop);
        Assert.NotEqual(Guid.Empty, workshop.Id);
        Assert.Equal("FOT-001", workshop.Code);
        Assert.Equal("Photography 101", workshop.Title);
        Assert.Equal(20, workshop.Capacity);
        Assert.Equal(start, workshop.StartDateTime);
        Assert.Equal(end, workshop.EndDateTime);
        Assert.Equal(_instructorId, workshop.InstructorId);
    }

    [Fact]
    public void Create_WhenStartTimeIsInThePast_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var pastStart = DateTime.UtcNow.AddMinutes(-10);
        var end = DateTime.UtcNow.AddHours(1);

        // Act & Assert
        var ex = Assert.Throws<BusinessRuleViolationException>(() =>
            Workshop.Create("FOT-001", "Title", "Desc", _instructorId, pastStart, end, 10));

        Assert.Contains("future", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_WhenEndTimeIsBeforeOrEqualStartTime_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(1);
        var invalidEnd = start.AddMinutes(-5);

        // Act & Assert
        var ex = Assert.Throws<BusinessRuleViolationException>(() =>
            Workshop.Create("FOT-001", "Title", "Desc", _instructorId, start, invalidEnd, 10));

        Assert.Contains("after", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Create_WhenCapacityIsZeroOrNegative_ShouldThrowBusinessRuleViolationException(int invalidCapacity)
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(2);

        // Act & Assert
        var ex = Assert.Throws<BusinessRuleViolationException>(() =>
            Workshop.Create("FOT-001", "Title", "Desc", _instructorId, start, end, invalidCapacity));

        Assert.Contains("greater than 0", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenCodeIsEmpty_ShouldThrowBusinessRuleViolationException(string invalidCode)
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(2);

        // Act & Assert
        Assert.Throws<BusinessRuleViolationException>(() =>
            Workshop.Create(invalidCode, "Title", "Desc", _instructorId, start, end, 15));
    }

    [Fact]
    public void Reschedule_WhenValid_ShouldUpdateTimes()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(2);
        var workshop = Workshop.Create("FOT-001", "Title", "Desc", _instructorId, start, end, 15);

        var newStart = DateTime.UtcNow.AddDays(5);
        var newEnd = newStart.AddHours(3);

        // Act
        workshop.Reschedule(newStart, newEnd);

        // Assert
        Assert.Equal(newStart, workshop.StartDateTime);
        Assert.Equal(newEnd, workshop.EndDateTime);
    }
}
