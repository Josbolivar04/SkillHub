using System.Text.RegularExpressions;
using SkillHub.Domain.Common;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.ValueObjects;

public sealed class WorkshopCode : ValueObject
{
    private static readonly Regex CodePattern = new(@"^[A-Z0-9]+(-[A-Z0-9]+)*$", RegexOptions.Compiled);

    public string Value { get; }

    // EF Core constructor
    private WorkshopCode()
    {
        Value = string.Empty;
    }

    private WorkshopCode(string value)
    {
        Value = value;
    }

    public static WorkshopCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessRuleViolationException("Workshop code cannot be empty.");

        var trimmed = code.Trim().ToUpperInvariant();

        if (trimmed.Length < 3 || trimmed.Length > 20)
            throw new BusinessRuleViolationException("Workshop code length must be between 3 and 20 characters.");

        if (!CodePattern.IsMatch(trimmed))
            throw new BusinessRuleViolationException("Workshop code must contain only uppercase alphanumeric characters and hyphens (e.g. 'FOT-001').");

        return new WorkshopCode(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(WorkshopCode code) => code.Value;
}
