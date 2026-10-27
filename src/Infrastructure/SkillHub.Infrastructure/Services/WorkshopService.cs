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
