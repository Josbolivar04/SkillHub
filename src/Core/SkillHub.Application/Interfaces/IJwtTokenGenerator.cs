namespace SkillHub.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(string userId, string email, string fullName, IEnumerable<string> roles);
}
