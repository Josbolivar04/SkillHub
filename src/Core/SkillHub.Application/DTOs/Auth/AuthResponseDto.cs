namespace SkillHub.Application.DTOs.Auth;

public record AuthResponseDto(
    string Token,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    DateTime ExpiresAt
);
