using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Exceptions;
using SkillHub.Web.Models;

namespace SkillHub.Web.Controllers;

/// <summary>
/// Instructor (Employee role) panel: list own workshops, create, edit and view enrolled participants.
/// </summary>
[Authorize(Roles = Roles.Employee)]
public class InstructorController : Controller
{
    private readonly IWorkshopService _workshopService;
    private readonly ILogger<InstructorController> _logger;

    public InstructorController(IWorkshopService workshopService, ILogger<InstructorController> logger)
    {
        _workshopService = workshopService;
        _logger = logger;
    }

    private string? CurrentEmail => User.FindFirst(ClaimTypes.Email)?.Value;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (CurrentEmail is null) return RedirectToAction("Login", "Account");

        try
        {
            var workshops = await _workshopService.GetWorkshopsByInstructorAsync(CurrentEmail, cancellationToken);
            return View(workshops);
        }
        catch (BusinessRuleViolationException ex)
        {
            // The user has the Employee role but no Instructor profile linked to their email.
            _logger.LogWarning(ex, "Employee {Email} has no instructor profile", CurrentEmail);
            TempData["ErrorMessage"] = "Tu usuario no tiene un perfil de instructor asociado. Contacta al administrador.";
            return View(Array.Empty<WorkshopDto>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("WorkshopForm", new WorkshopFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkshopFormViewModel model, CancellationToken cancellationToken)
    {
        if (CurrentEmail is null) return RedirectToAction("Login", "Account");

        model.Id = null;
        if (!ModelState.IsValid)
        {
            return View("WorkshopForm", model);
        }

        try
        {
            var created = await _workshopService.CreateWorkshopAsync(new CreateWorkshopDto(
                model.Code,
                model.Title,
                model.Description,
                CurrentEmail, // a workshop created from the panel is always assigned to the logged-in instructor
                model.StartDateTime,
                model.EndDateTime,
                model.Capacity), cancellationToken);

            TempData["SuccessMessage"] = $"El taller '{created.Title}' ({created.Code}) fue creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleViolationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("WorkshopForm", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workshop {Code}", model.Code);
            ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al crear el taller.");
            return View("WorkshopForm", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var workshop = await GetOwnedWorkshopAsync(id, cancellationToken);
        if (workshop is null) return Forbid();

        return View("WorkshopForm", new WorkshopFormViewModel
        {
            Id = workshop.Id,
            Code = workshop.Code,
            Title = workshop.Title,
            Description = workshop.Description,
            StartDateTime = workshop.StartDateTime,
            EndDateTime = workshop.EndDateTime,
            Capacity = workshop.Capacity,
            ActiveEnrollments = workshop.ActiveEnrollments
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, WorkshopFormViewModel model, CancellationToken cancellationToken)
    {
        var workshop = await GetOwnedWorkshopAsync(id, cancellationToken);
        if (workshop is null) return Forbid();

        // Code is immutable and enrollment count comes from the database, never from the form.
        model.Id = id;
        model.Code = workshop.Code;
        model.ActiveEnrollments = workshop.ActiveEnrollments;
        ModelState.Remove(nameof(model.Code));
        ModelState.Remove(nameof(model.ActiveEnrollments));

        if (!ModelState.IsValid)
        {
            return View("WorkshopForm", model);
        }

        try
        {
            var updated = await _workshopService.UpdateWorkshopAsync(id, new UpdateWorkshopDto(
                model.Title,
                model.Description,
                model.StartDateTime,
                model.EndDateTime,
                model.Capacity), CurrentEmail!, cancellationToken);

            TempData["SuccessMessage"] = $"Los cambios del taller '{updated.Title}' se guardaron correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleViolationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("WorkshopForm", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workshop {Id}", id);
            ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al guardar los cambios.");
            return View("WorkshopForm", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Attendees(Guid id, CancellationToken cancellationToken)
    {
        var workshop = await GetOwnedWorkshopAsync(id, cancellationToken);
        if (workshop is null) return Forbid();

        var attendees = await _workshopService.GetWorkshopAttendeesAsync(id, cancellationToken);
        ViewBag.Workshop = workshop;
        return View(attendees);
    }

    /// <summary>
    /// Returns the workshop only if it belongs to the logged-in instructor; otherwise null.
    /// </summary>
    private async Task<WorkshopDto?> GetOwnedWorkshopAsync(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentEmail is null) return null;
        if (!await _workshopService.IsWorkshopOwnedByInstructorAsync(id, CurrentEmail, cancellationToken)) return null;
        return await _workshopService.GetByIdAsync(id, cancellationToken);
    }
}
