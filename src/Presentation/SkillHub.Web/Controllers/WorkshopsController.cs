using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Web.Controllers;

public class WorkshopsController : Controller
{
    private readonly IWorkshopService _workshopService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILogger<WorkshopsController> _logger;

    public WorkshopsController(
        IWorkshopService workshopService,
        IEnrollmentService enrollmentService,
        ILogger<WorkshopsController> logger)
    {
        _workshopService = workshopService;
        _enrollmentService = enrollmentService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        var workshops = await _workshopService.GetFutureWorkshopsAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            workshops = workshops.Where(w =>
                w.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                w.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (w.InstructorName != null && w.InstructorName.Contains(term, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        ViewBag.SearchTerm = search;
        return View(workshops);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var workshop = await _workshopService.GetByIdAsync(id, cancellationToken);
        if (workshop == null)
        {
            return NotFound();
        }

        return View(workshop);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Participant)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(Guid id, CancellationToken cancellationToken)
    {
        var userIdentifier = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdentifier))
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            var result = await _enrollmentService.EnrollAsync(userIdentifier, new CreateEnrollmentDto(id), cancellationToken);
            TempData["SuccessMessage"] = $"Congratulations! You have successfully enrolled in '{result.WorkshopTitle}' ({result.WorkshopCode}).";
            return RedirectToAction("MyEnrollments", "Enrollments");
        }
        catch (BusinessRuleViolationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling participant {User} in workshop {Id}", userIdentifier, id);
            TempData["ErrorMessage"] = "An unexpected error occurred while attempting to enroll. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
