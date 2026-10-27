using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Exceptions;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
[Produces("application/json")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICertificateService _certificateService;
    private readonly ILogger<EnrollmentsController> _logger;

    public EnrollmentsController(
        IEnrollmentService enrollmentService,
        ICertificateService certificateService,
        ILogger<EnrollmentsController> logger)
    {
        _enrollmentService = enrollmentService;
        _certificateService = certificateService;
        _logger = logger;
    }

    /// <summary>
    /// Enrolls the authenticated participant into a workshop.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireParticipantRole")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Enroll([FromBody] CreateEnrollmentDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User identity could not be verified." });
        }

        try
        {
            var enrollment = await _enrollmentService.EnrollAsync(userId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, enrollment);
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error enrolling user {UserId} in workshop {WorkshopId}", userId, request.WorkshopId);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your enrollment." });
        }
    }

    /// <summary>
    /// Gets all enrollments for the authenticated participant.
    /// </summary>
    [HttpGet("mine")]
    [Authorize(Policy = "RequireParticipantRole")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyEnrollments(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User identity could not be verified." });
        }

        var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(userId, cancellationToken);
        return Ok(enrollments);
    }

    /// <summary>
    /// Cancels an enrollment (Participant owner or Employee).
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var userIdentifier = GetCurrentUserIdentifier();
        if (string.IsNullOrEmpty(userIdentifier))
        {
            return Unauthorized(new { message = "User identity could not be verified." });
        }

        var isEmployee = User.IsInRole(Roles.Employee);

        try
        {
            var result = await _enrollmentService.CancelEnrollmentAsync(id, userIdentifier, isEmployee, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error cancelling enrollment {EnrollmentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while cancelling the enrollment." });
        }
    }

    /// <summary>
    /// Mass import enrollments from an Excel file (Employee Only).
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = "RequireEmployeeRole")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Please upload a valid Excel file (.xlsx)." });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _enrollmentService.ImportEnrollmentsFromExcelAsync(stream, cancellationToken);
            return Ok(result);
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing enrollments from Excel file");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while importing the Excel file." });
        }
    }

    /// <summary>
    /// Downloads an enrollment confirmation PDF certificate (Owner or Employee).
    /// </summary>
    [HttpGet("{id:guid}/certificate")]
    [Authorize]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadCertificate(Guid id, CancellationToken cancellationToken)
    {
        var userIdentifier = GetCurrentUserIdentifier();
        if (string.IsNullOrEmpty(userIdentifier))
        {
            return Unauthorized(new { message = "User identity could not be verified." });
        }

        var isEmployee = User.IsInRole(Roles.Employee);

        try
        {
            var pdfBytes = await _certificateService.GenerateEnrollmentCertificateAsync(id, userIdentifier, isEmployee, cancellationToken);
            return File(pdfBytes, "application/pdf", $"Constancia_Inscripcion_{id}.pdf");
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating certificate for enrollment {EnrollmentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while generating the certificate." });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? User.FindFirst(ClaimTypes.Email)?.Value;
    }

    private string? GetCurrentUserIdentifier()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value;
    }
}
