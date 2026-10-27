using SkillHub.Application.DTOs.Reports;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.Repositories;

namespace SkillHub.Infrastructure.Services;

public class WorkshopService : IWorkshopService
{
    private readonly IWorkshopRepository _workshopRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkshopService(
        IWorkshopRepository workshopRepository,
        IInstructorRepository instructorRepository,
        IUnitOfWork unitOfWork)
    {
        _workshopRepository = workshopRepository;
        _instructorRepository = instructorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<WorkshopDto>> GetFutureWorkshopsAsync(CancellationToken cancellationToken = default)
    {
        var workshops = await _workshopRepository.GetUpcomingAsync(cancellationToken);

        return workshops.Select(MapToDto).ToList();
    }

    public async Task<WorkshopDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workshop = await _workshopRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (workshop == null)
            return null;

        return MapToDto(workshop);
    }

    public async Task<WorkshopDto> CreateWorkshopAsync(CreateWorkshopDto request, CancellationToken cancellationToken = default)
    {
        // 1. Validate instructor existence by email
        var normalizedEmail = request.InstructorEmail.Trim().ToLowerInvariant();
        var instructor = await _instructorRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (instructor == null)
        {
            throw new BusinessRuleViolationException($"Instructor with email '{request.InstructorEmail}' does not exist.");
        }

        // 2. Validate code uniqueness
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var exists = await _workshopRepository.ExistsWithCodeAsync(normalizedCode, cancellationToken: cancellationToken);
        if (exists)
        {
            throw new BusinessRuleViolationException($"A workshop with code '{request.Code}' already exists.");
        }

        // 3. Create Workshop (Domain entity enforces invariants: future start time, end after start, capacity > 0)
        var workshop = Workshop.Create(
            normalizedCode,
            request.Title,
            request.Description ?? string.Empty,
            instructor.Id,
            request.StartDateTime,
            request.EndDateTime,
            request.Capacity);

        await _workshopRepository.AddAsync(workshop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkshopDto(
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.Description,
            instructor.Id,
            instructor.FullName,
            instructor.Email,
            workshop.StartDateTime,
            workshop.EndDateTime,
            workshop.Capacity,
            0,
            workshop.Capacity
        );
    }

    public async Task<IReadOnlyList<WorkshopAttendeeDto>> GetWorkshopAttendeesAsync(Guid workshopId, CancellationToken cancellationToken = default)
    {
        var workshop = await _workshopRepository.GetByIdWithDetailsAsync(workshopId, cancellationToken);
        if (workshop == null)
        {
            throw new BusinessRuleViolationException($"Workshop with ID '{workshopId}' was not found.");
        }

        return workshop.Enrollments.Select(e => new WorkshopAttendeeDto(
            e.Id,
            e.ParticipantId,
            e.Participant?.FullName ?? "Unknown",
            e.Participant?.Email ?? "Unknown",
            e.EnrolledAt,
            e.Status.ToString()
        )).OrderByDescending(a => a.EnrolledAt).ToList();
    }

    public async Task<IReadOnlyList<TopWorkshopReportDto>> GetTopWorkshopsReportAsync(CancellationToken cancellationToken = default)
    {
        var workshops = await _workshopRepository.GetAllAsync(cancellationToken);

        // Order by active enrollments descending
        return workshops.Select(w =>
        {
            var activeCount = w.GetActiveEnrollmentCount();
            var availableSlots = Math.Max(0, w.Capacity - activeCount);
            var occupancy = w.Capacity > 0 ? Math.Round((double)activeCount / w.Capacity * 100, 2) : 0;

            return new TopWorkshopReportDto(
                w.Id,
                w.Code,
                w.Title,
                w.Instructor?.FullName,
                w.StartDateTime,
                w.EndDateTime,
                w.Capacity,
                activeCount,
                availableSlots,
                occupancy
            );
        })
        .OrderByDescending(r => r.ActiveEnrollments)
        .ThenByDescending(r => r.OccupancyPercentage)
        .ToList();
    }

    public async Task<IReadOnlyList<WorkshopDto>> GetWorkshopsByInstructorAsync(string instructorEmail, CancellationToken cancellationToken = default)
    {
        var instructor = await GetInstructorOrThrowAsync(instructorEmail, cancellationToken);
        var workshops = await _workshopRepository.GetAllAsync(cancellationToken);

        return workshops
            .Where(w => w.InstructorId == instructor.Id)
            .OrderByDescending(w => w.StartDateTime > DateTime.UtcNow) // upcoming first
            .ThenBy(w => w.StartDateTime)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<WorkshopDto> UpdateWorkshopAsync(Guid workshopId, UpdateWorkshopDto request, string instructorEmail, CancellationToken cancellationToken = default)
    {
        var instructor = await GetInstructorOrThrowAsync(instructorEmail, cancellationToken);

        var workshop = await _workshopRepository.GetByIdWithDetailsAsync(workshopId, cancellationToken)
            ?? throw new BusinessRuleViolationException($"Workshop with ID '{workshopId}' was not found.");

        if (workshop.InstructorId != instructor.Id)
        {
            throw new BusinessRuleViolationException("Only the instructor assigned to this workshop can edit it.");
        }

        var activeEnrollments = workshop.GetActiveEnrollmentCount();
        if (request.Capacity < activeEnrollments)
        {
            throw new BusinessRuleViolationException(
                $"Capacity cannot be lower than the current number of active enrollments ({activeEnrollments}).");
        }

        // Only re-validate the schedule when it actually changes, so past workshops can still be corrected.
        // Compared at minute precision because HTML datetime-local inputs drop seconds.
        if (TruncateToMinute(request.StartDateTime) != TruncateToMinute(workshop.StartDateTime) ||
            TruncateToMinute(request.EndDateTime) != TruncateToMinute(workshop.EndDateTime))
        {
            workshop.Reschedule(request.StartDateTime, request.EndDateTime);
        }

        workshop.UpdateDetails(request.Title, request.Description ?? string.Empty, request.Capacity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(workshop);
    }

    public async Task<bool> IsWorkshopOwnedByInstructorAsync(Guid workshopId, string instructorEmail, CancellationToken cancellationToken = default)
    {
        var instructor = await _instructorRepository.GetByEmailAsync(instructorEmail.Trim().ToLowerInvariant(), cancellationToken);
        if (instructor == null)
            return false;

        var workshop = await _workshopRepository.GetByIdAsync(workshopId, cancellationToken);
        return workshop != null && workshop.InstructorId == instructor.Id;
    }

    private async Task<Instructor> GetInstructorOrThrowAsync(string instructorEmail, CancellationToken cancellationToken)
    {
        var normalizedEmail = instructorEmail.Trim().ToLowerInvariant();
        return await _instructorRepository.GetByEmailAsync(normalizedEmail, cancellationToken)
            ?? throw new BusinessRuleViolationException($"Instructor with email '{instructorEmail}' does not exist.");
    }

    private static DateTime TruncateToMinute(DateTime value) =>
        new(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind);

    private static WorkshopDto MapToDto(Workshop w)
    {
        var activeCount = w.GetActiveEnrollmentCount();
        var availableSlots = Math.Max(0, w.Capacity - activeCount);

        return new WorkshopDto(
            w.Id,
            w.Code,
            w.Title,
            w.Description,
            w.InstructorId,
            w.Instructor?.FullName,
            w.Instructor?.Email,
            w.StartDateTime,
            w.EndDateTime,
            w.Capacity,
            activeCount,
            availableSlots
        );
    }
}
