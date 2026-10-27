using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.Repositories;

namespace SkillHub.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService, IExcelImporter
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IWorkshopRepository _workshopRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IWorkshopRepository workshopRepository,
        IParticipantRepository participantRepository,
        IUnitOfWork unitOfWork)
    {
        _enrollmentRepository = enrollmentRepository;
        _workshopRepository = workshopRepository;
        _participantRepository = participantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EnrollmentDto> EnrollAsync(string userId, CreateEnrollmentDto request, CancellationToken cancellationToken = default)
    {
        // 1. Resolve participant
        var participant = await ResolveParticipantAsync(userId, cancellationToken);
        if (participant == null)
        {
            throw new BusinessRuleViolationException("Participant profile not found for the authenticated user.");
        }

        // 2. Resolve workshop
        var workshop = await _workshopRepository.GetByIdWithDetailsAsync(request.WorkshopId, cancellationToken);
        if (workshop == null)
        {
            throw new BusinessRuleViolationException("The specified workshop does not exist.");
        }

        // 3. Rule: Future Only (Cannot enroll in past or already started workshops)
        if (workshop.StartDateTime <= DateTime.UtcNow)
        {
            throw new BusinessRuleViolationException("Cannot enroll in a workshop that has already started or is in the past.");
        }

        // 4. Rule: Capacity Check (Active enrollments cannot exceed Capacity)
        var activeEnrollmentsCount = await _enrollmentRepository.GetActiveEnrollmentCountAsync(workshop.Id, cancellationToken);
        if (activeEnrollmentsCount >= workshop.Capacity)
        {
            throw new BusinessRuleViolationException($"Workshop is full. Capacity of {workshop.Capacity} has been reached.");
        }

        // 5. Rule: No Duplicates (Participant cannot have an active enrollment in the same workshop)
        var hasActiveEnrollment = await _enrollmentRepository.HasActiveEnrollmentAsync(participant.Id, workshop.Id, cancellationToken);
        if (hasActiveEnrollment)
        {
            throw new BusinessRuleViolationException("You already have an active enrollment in this workshop.");
        }

        // 6. Rule: No Schedule Overlaps
        // Participant cannot enroll in a workshop whose time interval [Start, End] overlaps with another active workshop
        var activeEnrollments = await _enrollmentRepository.GetActiveByParticipantIdAsync(participant.Id, cancellationToken);
        foreach (var active in activeEnrollments)
        {
            var other = active.Workshop ?? await _workshopRepository.GetByIdAsync(active.WorkshopId, cancellationToken);
            if (other != null)
            {
                // Two intervals [A_start, A_end] and [B_start, B_end] overlap if A_start < B_end and B_start < A_end
                var overlaps = workshop.StartDateTime < other.EndDateTime && other.StartDateTime < workshop.EndDateTime;
                if (overlaps)
                {
                    throw new BusinessRuleViolationException(
                        $"Schedule overlap: Workshop '{workshop.Title}' ({workshop.StartDateTime:g} - {workshop.EndDateTime:g}) conflicts with your active enrollment in '{other.Title}' ({other.StartDateTime:g} - {other.EndDateTime:g}).");
                }
            }
        }

        // 7. Create Enrollment
        var enrollment = Enrollment.Create(participant.Id, workshop.Id);
        await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EnrollmentDto(
            enrollment.Id,
            participant.Id,
            participant.FullName,
            participant.Email,
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.StartDateTime,
            workshop.EndDateTime,
            enrollment.Status.ToString(),
            enrollment.EnrolledAt,
            enrollment.CancelledAt,
            enrollment.CancelledBy
        );
    }

    public async Task<IReadOnlyList<EnrollmentDto>> GetMyEnrollmentsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var participant = await ResolveParticipantAsync(userId, cancellationToken);
        if (participant == null)
        {
            return Array.Empty<EnrollmentDto>();
        }

        var enrollments = await _enrollmentRepository.GetByParticipantIdAsync(participant.Id, cancellationToken);

        return enrollments.Select(e => new EnrollmentDto(
            e.Id,
            e.ParticipantId,
            e.Participant?.FullName ?? participant.FullName,
            e.Participant?.Email ?? participant.Email,
            e.WorkshopId,
            e.Workshop?.Code ?? string.Empty,
            e.Workshop?.Title ?? string.Empty,
            e.Workshop?.StartDateTime ?? DateTime.MinValue,
            e.Workshop?.EndDateTime ?? DateTime.MinValue,
            e.Status.ToString(),
            e.EnrolledAt,
            e.CancelledAt,
            e.CancelledBy
        )).ToList();
    }

    public async Task<EnrollmentDto> CancelEnrollmentAsync(Guid enrollmentId, string userIdentifier, bool isEmployee, CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment == null)
        {
            throw new BusinessRuleViolationException("Enrollment not found.");
        }

        if (enrollment.Status == EnrollmentStatus.Cancelled)
        {
            throw new BusinessRuleViolationException("Enrollment is already cancelled.");
        }

        // Check ownership if not employee
        Participant? participant = null;
        if (!isEmployee)
        {
            participant = await ResolveParticipantAsync(userIdentifier, cancellationToken);
            if (participant == null || enrollment.ParticipantId != participant.Id)
            {
                throw new UnauthorizedAccessException("You are not authorized to cancel this enrollment.");
            }
        }
        else
        {
            participant = enrollment.Participant ?? await _participantRepository.GetByIdAsync(enrollment.ParticipantId, cancellationToken);
        }

        var workshop = enrollment.Workshop ?? await _workshopRepository.GetByIdAsync(enrollment.WorkshopId, cancellationToken);
        if (workshop == null)
        {
            throw new BusinessRuleViolationException("Associated workshop not found.");
        }

        var now = DateTime.UtcNow;

        // Rule: Future Only (Cannot cancel past workshops)
        if (workshop.StartDateTime <= now)
        {
            throw new BusinessRuleViolationException("Cannot cancel an enrollment for a workshop that has already started or is in the past.");
        }

        // Rule: Cancellation window
        // - Participant can cancel up to 24 hours before workshop start time
        // - Employee can cancel anytime before workshop start time
        if (!isEmployee)
        {
            var cutoffTime = workshop.StartDateTime.AddHours(-24);
            if (now > cutoffTime)
            {
                throw new BusinessRuleViolationException("Participants can only cancel up to 24 hours before the workshop start time.");
            }
        }

        enrollment.Cancel(userIdentifier);
        _enrollmentRepository.Update(enrollment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EnrollmentDto(
            enrollment.Id,
            enrollment.ParticipantId,
            participant?.FullName,
            participant?.Email,
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.StartDateTime,
            workshop.EndDateTime,
            enrollment.Status.ToString(),
            enrollment.EnrolledAt,
            enrollment.CancelledAt,
            enrollment.CancelledBy
        );
    }

    public async Task<ImportResultDto> ImportEnrollmentsFromExcelAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("SkillHub");
        using var package = new OfficeOpenXml.ExcelPackage(fileStream);

        var worksheet = package.Workbook.Worksheets["Enrollments"] ?? package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null || worksheet.Dimension == null)
        {
            throw new BusinessRuleViolationException("The uploaded Excel file does not contain valid data or worksheets.");
        }

        var errors = new List<ImportErrorDto>();
        var importedCount = 0;
        var totalRows = 0;

        // Cache workshops and participants to optimize batch performance
        var allParticipants = (await _participantRepository.GetAllAsync(cancellationToken))
            .ToDictionary(p => p.Email.ToLowerInvariant(), p => p);

        var allWorkshops = (await _workshopRepository.GetAllAsync(cancellationToken))
            .ToDictionary(w => w.Code.ToUpperInvariant(), w => w);

        // Keep track of new enrollments created during this import session
        var newlyCreatedEnrollments = new List<Enrollment>();

        for (int r = 2; r <= worksheet.Dimension.Rows; r++)
        {
            var emailText = worksheet.Cells[r, 1].Text?.Trim().ToLowerInvariant();
            var codeText = worksheet.Cells[r, 2].Text?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(emailText) && string.IsNullOrWhiteSpace(codeText))
            {
                continue; // Skip empty rows
            }

            totalRows++;

            if (string.IsNullOrWhiteSpace(emailText))
            {
                errors.Add(new ImportErrorDto(r, emailText, codeText, "ParticipantEmail is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(codeText))
            {
                errors.Add(new ImportErrorDto(r, emailText, codeText, "WorkshopCode is required."));
                continue;
            }

            // 1. Check participant exists
            if (!allParticipants.TryGetValue(emailText, out var participant))
            {
                errors.Add(new ImportErrorDto(r, emailText, codeText, $"Participant with email '{emailText}' does not exist."));
                continue;
            }

            // 2. Check workshop exists
            if (!allWorkshops.TryGetValue(codeText, out var workshop))
            {
                errors.Add(new ImportErrorDto(r, emailText, codeText, $"Workshop with code '{codeText}' does not exist."));
                continue;
            }

            // 3. Rule: Future Only
            if (workshop.StartDateTime <= DateTime.UtcNow)
            {
                errors.Add(new ImportErrorDto(r, emailText, codeText, "Cannot enroll in a workshop that has already started or is in the past."));
                continue;
            }

            // 4. Rule: Capacity Check
            var currentActiveInDb = await _enrollmentRepository.GetActiveEnrollmentCountAsync(workshop.Id, cancellationToken);
            var newlyAddedForWorkshop = newlyCreatedEnrollments.Count(e => e.WorkshopId == workshop.Id);
            if (currentActiveInDb + newlyAddedForWorkshop >= workshop.Capacity)
            {
                errors.Add(new ImportErrorDto(r, emailText, codeText, $"Workshop capacity of {workshop.Capacity} has been reached."));
                continue;
            }

            // 5. Rule: No Duplicates
            var hasActiveInDb = await _enrollmentRepository.HasActiveEnrollmentAsync(participant.Id, workshop.Id, cancellationToken);
            var hasActiveInBatch = newlyCreatedEnrollments.Any(e => e.ParticipantId == participant.Id && e.WorkshopId == workshop.Id);
            if (hasActiveInDb || hasActiveInBatch)
            {
                errors.Add(new ImportErrorDto(r, emailText, codeText, "Participant already has an active enrollment in this workshop."));
                continue;
            }

            // 6. Rule: No Schedule Overlaps
            var existingActive = await _enrollmentRepository.GetActiveByParticipantIdAsync(participant.Id, cancellationToken);
            var batchActiveForParticipant = newlyCreatedEnrollments.Where(e => e.ParticipantId == participant.Id).ToList();

            var overlapDetected = false;
            foreach (var active in existingActive)
            {
                var other = active.Workshop ?? (allWorkshops.Values.FirstOrDefault(w => w.Id == active.WorkshopId));
                if (other != null && workshop.StartDateTime < other.EndDateTime && other.StartDateTime < workshop.EndDateTime)
                {
                    errors.Add(new ImportErrorDto(r, emailText, codeText, $"Schedule overlap: Workshop conflicts with active enrollment in '{other.Title}'."));
                    overlapDetected = true;
                    break;
                }
            }

            if (overlapDetected) continue;

            foreach (var newBatch in batchActiveForParticipant)
            {
                var other = allWorkshops.Values.FirstOrDefault(w => w.Id == newBatch.WorkshopId);
                if (other != null && workshop.StartDateTime < other.EndDateTime && other.StartDateTime < workshop.EndDateTime)
                {
                    errors.Add(new ImportErrorDto(r, emailText, codeText, $"Schedule overlap: Workshop conflicts with another workshop in the same import file ('{other.Title}')."));
                    overlapDetected = true;
                    break;
                }
            }

            if (overlapDetected) continue;

            // Row is valid! Create enrollment
            var enrollment = Enrollment.Create(participant.Id, workshop.Id);
            newlyCreatedEnrollments.Add(enrollment);
            await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
            importedCount++;
        }

        if (newlyCreatedEnrollments.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ImportResultDto(
            totalRows,
            importedCount,
            errors.Count,
            errors
        );
    }

    private async Task<Participant?> ResolveParticipantAsync(string userIdentifier, CancellationToken cancellationToken)
    {
        var participant = await _participantRepository.GetByUserIdAsync(userIdentifier, cancellationToken);
        if (participant != null) return participant;

        if (Guid.TryParse(userIdentifier, out var guidId))
        {
            participant = await _participantRepository.GetByIdAsync(guidId, cancellationToken);
            if (participant != null) return participant;
        }

        return await _participantRepository.GetByEmailAsync(userIdentifier, cancellationToken);
    }
}
