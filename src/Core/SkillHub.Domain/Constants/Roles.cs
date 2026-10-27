namespace SkillHub.Domain.Constants;

public static class Roles
{
    public const string Employee = "Employee";
    public const string Participant = "Participant";

    public static readonly IReadOnlyList<string> All = new[] { Employee, Participant };
}
