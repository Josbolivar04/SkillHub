using SkillHub.Domain.Constants;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;
using Xunit;

namespace SkillHub.Domain.UnitTests;

public class InstructorAndParticipantTests
{
    [Fact]
    public void Roles_ShouldContainEmployeeAndParticipant()
    {
        Assert.Equal("Employee", Roles.Employee);
        Assert.Equal("Participant", Roles.Participant);
        Assert.Contains(Roles.Employee, Roles.All);
        Assert.Contains(Roles.Participant, Roles.All);
    }

    [Fact]
    public void CreateInstructor_WithValidData_ShouldSucceed()
    {
        var instructor = Instructor.Create("Jane Doe", "jane@skillhub.com", "Cloud Computing");

        Assert.NotNull(instructor);
        Assert.NotEqual(Guid.Empty, instructor.Id);
        Assert.Equal("Jane Doe", instructor.FullName);
        Assert.Equal("jane@skillhub.com", instructor.Email);
        Assert.Equal("Cloud Computing", instructor.Specialty);
        Assert.Empty(instructor.Workshops);
    }

    [Theory]
    [InlineData("", "jane@skillhub.com", "Specialty")]
    [InlineData("Jane Doe", "", "Specialty")]
    [InlineData("Jane Doe", "jane@skillhub.com", "")]
    public void CreateInstructor_WithMissingData_ShouldThrowBusinessRuleViolationException(
        string name, string email, string specialty)
    {
        Assert.Throws<BusinessRuleViolationException>(() =>
            Instructor.Create(name, email, specialty));
    }

    [Fact]
    public void CreateParticipant_WithValidData_ShouldSucceed()
    {
        var participant = Participant.Create("John Smith", "john@skillhub.com");

        Assert.NotNull(participant);
        Assert.NotEqual(Guid.Empty, participant.Id);
        Assert.Equal("John Smith", participant.FullName);
        Assert.Equal("john@skillhub.com", participant.Email);
        Assert.True((DateTime.UtcNow - participant.RegisteredAt).TotalSeconds < 5);
        Assert.Empty(participant.Enrollments);
    }

    [Theory]
    [InlineData("", "john@skillhub.com")]
    [InlineData("John Smith", "")]
    public void CreateParticipant_WithMissingData_ShouldThrowBusinessRuleViolationException(
        string name, string email)
    {
        Assert.Throws<BusinessRuleViolationException>(() =>
            Participant.Create(name, email));
    }
}
