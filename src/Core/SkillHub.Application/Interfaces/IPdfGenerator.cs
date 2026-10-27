namespace SkillHub.Application.Interfaces;

/// <summary>
/// Abstraction for generating PDF documents and certificates using QuestPDF.
/// </summary>
public interface IPdfGenerator
{
    Task<byte[]> GenerateEnrollmentCertificateAsync(Guid enrollmentId, string userIdentifier, bool isEmployee, CancellationToken cancellationToken = default);
}
