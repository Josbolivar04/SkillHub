using System.Text.RegularExpressions;
using SkillHub.Domain.Common;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.ValueObjects;

public sealed class EmailAddress : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress()
    {
        Value = string.Empty;
    }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleViolationException("Email address cannot be empty.");

        var trimmed = email.Trim().ToLowerInvariant();

        if (trimmed.Length > 256)
            throw new BusinessRuleViolationException("Email address cannot exceed 256 characters.");

        if (!EmailRegex.IsMatch(trimmed))
            throw new BusinessRuleViolationException("Email address format is invalid.");

        return new EmailAddress(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(EmailAddress email) => email.Value;
}
