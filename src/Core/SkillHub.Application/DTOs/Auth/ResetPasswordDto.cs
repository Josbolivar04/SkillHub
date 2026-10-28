using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Auth;

public record ResetPasswordDto(
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    string Email,

    [Required(ErrorMessage = "Token is required.")]
    string Token,

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    string NewPassword
);
