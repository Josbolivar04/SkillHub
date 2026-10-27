using SkillHub.Application.DTOs.Enrollments;

namespace SkillHub.Application.Interfaces;

/// <summary>
/// Abstraction for bulk importing enrollment data from Excel spreadsheets using EPPlus.
/// </summary>
public interface IExcelImporter
{
    Task<ImportResultDto> ImportEnrollmentsFromExcelAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
