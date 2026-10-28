using Microsoft.AspNetCore.Identity;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;
using SkillHub.Infrastructure.Data;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new BusinessRuleViolationException($"A user with email '{request.Email}' already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim().ToLowerInvariant(),
            Email = request.Email.Trim().ToLowerInvariant(),
            FullName = request.FullName.Trim()
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleViolationException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, Roles.Participant);

        // Synchronize with Participant domain entity
        var participant = Participant.Create(user.FullName, user.Email, user.Id);
        await _context.Participants.AddAsync(participant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var roles = new[] { Roles.Participant };
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.FullName, roles);

        return new AuthResponseDto(token, user.Email, user.FullName, roles, expiresAt);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user.Id, user.Email ?? string.Empty, user.FullName, roles);

        return new AuthResponseDto(token, user.Email ?? string.Empty, user.FullName, roles.ToList(), expiresAt);
    }

    public async Task<string> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user == null)
        {
            throw new BusinessRuleViolationException($"No account was found with email '{email}'.");
        }

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user == null)
        {
            throw new BusinessRuleViolationException($"No account was found with email '{request.Email}'.");
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BusinessRuleViolationException($"Password reset failed: {errors}");
        }
    }
}
