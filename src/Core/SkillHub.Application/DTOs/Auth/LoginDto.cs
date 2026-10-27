using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs.Auth;

public record LoginDto(
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    string Email,

    [Required(ErrorMessage = "Password is required.")]
    string Password
);
