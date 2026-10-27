using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Reports;
using SkillHub.Application.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Produces("application/json")]
[Authorize(Policy = "RequireEmployeeRole")]
public class ReportsController : ControllerBase
{
    private readonly IWorkshopService _workshopService;

    public ReportsController(IWorkshopService workshopService)
    {
        _workshopService = workshopService;
    }

    /// <summary>
    /// Gets workshops ordered by active enrollments descending (Employee Only).
    /// </summary>
    [HttpGet("top-workshops")]
    [ProducesResponseType(typeof(IReadOnlyList<TopWorkshopReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopWorkshops(CancellationToken cancellationToken)
    {
        var report = await _workshopService.GetTopWorkshopsReportAsync(cancellationToken);
        return Ok(report);
    }
}
