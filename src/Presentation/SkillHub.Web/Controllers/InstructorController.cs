using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.DTOs.Reports;
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
    private readonly IExcelImporter _excelImporter;
    private readonly ILogger<InstructorController> _logger;

    public InstructorController(
        IWorkshopService workshopService,
        IExcelImporter excelImporter,
        ILogger<InstructorController> logger)
    {
        _workshopService = workshopService;
        _excelImporter = excelImporter;
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
    /// Displays workshops ranked by active enrollments descending for employee reporting.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Reports(CancellationToken cancellationToken)
    {
        var reports = await _workshopService.GetTopWorkshopsReportAsync(cancellationToken);
        return View(reports);
    }

    /// <summary>
    /// Displays the bulk enrollment from Excel interface.
    /// </summary>
    [HttpGet]
    public IActionResult ImportExcel()
    {
        return View((ImportResultDto?)null);
    }

    /// <summary>
    /// Generates and downloads a clean Excel template with ParticipantEmail and WorkshopCode columns.
    /// </summary>
    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("SkillHub");
        using var package = new OfficeOpenXml.ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Enrollments");

        // Headers
        ws.Cells[1, 1].Value = "ParticipantEmail";
        ws.Cells[1, 2].Value = "WorkshopCode";

        // Styling
        using (var range = ws.Cells[1, 1, 1, 2])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(67, 97, 238));
            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        // Example rows
        ws.Cells[2, 1].Value = "participante1@correo.com";
        ws.Cells[2, 2].Value = "FOT-001";
        ws.Cells[3, 1].Value = "participante2@correo.com";
        ws.Cells[3, 2].Value = "COC-001";

        ws.Column(1).AutoFit();
        ws.Column(2).AutoFit();

        var bytes = package.GetAsByteArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Plantilla_Inscripciones_SkillHub.xlsx");
    }

    /// <summary>
    /// Processes an uploaded Excel file, performs individual enrollment rules per row, and returns results with failures.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportExcel(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Por favor selecciona un archivo Excel (.xlsx) para continuar.");
            return View((ImportResultDto?)null);
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "El formato del archivo debe ser Excel (.xlsx).");
            return View((ImportResultDto?)null);
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _excelImporter.ImportEnrollmentsFromExcelAsync(stream, cancellationToken);
            return View(result);
        }
        catch (BusinessRuleViolationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View((ImportResultDto?)null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bulk Excel import");
            ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado al procesar el archivo Excel.");
            return View((ImportResultDto?)null);
        }
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
