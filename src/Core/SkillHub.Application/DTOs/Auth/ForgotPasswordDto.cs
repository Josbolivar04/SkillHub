using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Auth;

public record ForgotPasswordDto(
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    string Email
);
