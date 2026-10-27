using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Exceptions;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/v1/workshops")]
[Produces("application/json")]
public class WorkshopsController : ControllerBase
{
    private readonly IWorkshopService _workshopService;
    private readonly ILogger<WorkshopsController> _logger;

    public WorkshopsController(IWorkshopService workshopService, ILogger<WorkshopsController> logger)
    {
        _workshopService = workshopService;
        _logger = logger;
    }

    /// <summary>
    /// Returns future workshops only with calculated available slots (Public).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<WorkshopDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFutureWorkshops(CancellationToken cancellationToken)
    {
        var workshops = await _workshopService.GetFutureWorkshopsAsync(cancellationToken);
        return Ok(workshops);
    }

    /// <summary>
    /// Returns details of a workshop by ID with available slots (Public).
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WorkshopDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkshopById(Guid id, CancellationToken cancellationToken)
    {
        var workshop = await _workshopService.GetByIdAsync(id, cancellationToken);
        if (workshop == null)
        {
            return NotFound(new { message = $"Workshop with ID '{id}' was not found." });
        }

        return Ok(workshop);
    }

    /// <summary>
    /// Creates a new workshop (Employee Only).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireEmployeeRole")]
    [ProducesResponseType(typeof(WorkshopDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateWorkshop([FromBody] CreateWorkshopDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _workshopService.CreateWorkshopAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetWorkshopById), new { id = created.Id }, created);
        }
        catch (BusinessRuleViolationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating workshop '{Code}'", request.Code);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the workshop." });
        }
    }

    /// <summary>
    /// Gets all enrolled attendees of a workshop (Employee Only).
    /// </summary>
    [HttpGet("{id:guid}/enrollments")]
    [Authorize(Policy = "RequireEmployeeRole")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkshopAttendeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkshopEnrollments(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var attendees = await _workshopService.GetWorkshopAttendeesAsync(id, cancellationToken);
            return Ok(attendees);
        }
        catch (BusinessRuleViolationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
