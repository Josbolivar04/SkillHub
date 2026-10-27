using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Web.Controllers;

[Authorize]
public class EnrollmentsController : Controller
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

    [HttpGet]
    [Authorize(Roles = Roles.Participant)]
    public async Task<IActionResult> MyEnrollments(CancellationToken cancellationToken)
    {
        var userIdentifier = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdentifier))
        {
            return RedirectToAction("Login", "Account");
        }

        var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(userIdentifier, cancellationToken);
        return View(enrollments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var userIdentifier = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdentifier))
        {
            return RedirectToAction("Login", "Account");
        }

        var isEmployee = User.IsInRole(Roles.Employee);

        try
        {
            await _enrollmentService.CancelEnrollmentAsync(id, userIdentifier, isEmployee, cancellationToken);
            TempData["SuccessMessage"] = "Your enrollment has been successfully cancelled.";
        }
        catch (BusinessRuleViolationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling enrollment {Id}", id);
            TempData["ErrorMessage"] = "An error occurred while cancelling your enrollment.";
        }

        return RedirectToAction(nameof(MyEnrollments));
    }

    [HttpGet]
    public async Task<IActionResult> Certificate(Guid id, CancellationToken cancellationToken)
    {
        var userIdentifier = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdentifier))
        {
            return RedirectToAction("Login", "Account");
        }

        var isEmployee = User.IsInRole(Roles.Employee);

        try
        {
            var pdfBytes = await _certificateService.GenerateEnrollmentCertificateAsync(id, userIdentifier, isEmployee, cancellationToken);
            return File(pdfBytes, "application/pdf", $"Constancia_Inscripcion_{id}.pdf");
        }
        catch (BusinessRuleViolationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(MyEnrollments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating certificate for enrollment {Id}", id);
            TempData["ErrorMessage"] = "Could not generate certificate at this time.";
            return RedirectToAction(nameof(MyEnrollments));
        }
    }
}
