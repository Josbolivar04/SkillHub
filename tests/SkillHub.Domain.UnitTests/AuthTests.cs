using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Domain.Constants;
using SkillHub.Infrastructure.Services;
using Xunit;

namespace SkillHub.Domain.UnitTests;

public class AuthTests
{
    private readonly IConfiguration _configuration;

    public AuthTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "JwtSettings:Secret", "UnitTestingSecretKeyForGeneratingJwtTokens2026*SkillHub" },
            { "JwtSettings:Issuer", "SkillHubTestIssuer" },
            { "JwtSettings:Audience", "SkillHubTestAudience" },
            { "JwtSettings:ExpiryMinutes", "60" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void GenerateToken_ShouldIncludeUserIdEmailAndRoleClaims()
    {
        // Arrange
        var generator = new JwtTokenGenerator(_configuration);
        var userId = Guid.NewGuid().ToString();
        var email = "participant@skillhub.com";
        var fullName = "Test Participant";
        var roles = new[] { Roles.Participant };

        // Act
        var (token, expiresAt) = generator.GenerateToken(userId, email, fullName, roles);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(expiresAt > DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Check required claims: UserId, Email, Role
        var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "userId" || c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub);
        var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email || c.Type == JwtRegisteredClaimNames.Email);
        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == ClaimTypes.Role);

        Assert.NotNull(userIdClaim);
        Assert.Equal(userId, userIdClaim.Value);

        Assert.NotNull(emailClaim);
        Assert.Equal(email, emailClaim.Value);

        Assert.NotNull(roleClaim);
        Assert.Equal(Roles.Participant, roleClaim.Value);
    }

    [Fact]
    public void GenerateToken_WithEmployeeRole_ShouldIncludeEmployeeRoleClaim()
    {
        // Arrange
        var generator = new JwtTokenGenerator(_configuration);
        var userId = Guid.NewGuid().ToString();
        var email = "instructor@skillhub.com";
        var fullName = "Test Instructor";
        var roles = new[] { Roles.Employee };

        // Act
        var (token, _) = generator.GenerateToken(userId, email, fullName, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var roleClaims = jwt.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Contains(Roles.Employee, roleClaims);
    }

    [Fact]
    public void RegisterDto_ShouldCreateValidInstance()
    {
        var dto = new RegisterDto("Carlos Gomez", "carlos@correo.com", "Password2026*");

        Assert.Equal("Carlos Gomez", dto.FullName);
        Assert.Equal("carlos@correo.com", dto.Email);
        Assert.Equal("Password2026*", dto.Password);
    }

    [Fact]
    public void LoginDto_ShouldCreateValidInstance()
    {
        var dto = new LoginDto("carlos@correo.com", "Password2026*");

        Assert.Equal("carlos@correo.com", dto.Email);
        Assert.Equal("Password2026*", dto.Password);
    }

    [Fact]
    public void AuthResponseDto_ShouldCreateValidInstance()
    {
        var expiresAt = DateTime.UtcNow.AddHours(2);
        var dto = new AuthResponseDto("fake-token", "carlos@correo.com", "Carlos Gomez", new[] { Roles.Participant }, expiresAt);

        Assert.Equal("fake-token", dto.Token);
        Assert.Equal("carlos@correo.com", dto.Email);
        Assert.Equal("Carlos Gomez", dto.FullName);
        Assert.Contains(Roles.Participant, dto.Roles);
        Assert.Equal(expiresAt, dto.ExpiresAt);
    }
}
