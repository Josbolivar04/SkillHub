namespace SkillHub.Application.Interfaces;

public interface ICertificateService
{
    Task<byte[]> GenerateEnrollmentCertificateAsync(Guid enrollmentId, string userIdentifier, bool isEmployee, CancellationToken cancellationToken = default);
}
