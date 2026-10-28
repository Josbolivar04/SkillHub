using SkillHub.Application.DTOs.Auth;

namespace SkillHub.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
    Task<string> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default);
}
