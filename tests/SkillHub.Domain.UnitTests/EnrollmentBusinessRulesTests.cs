using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.Repositories;
using SkillHub.Infrastructure.Services;
using Xunit;

namespace SkillHub.Domain.UnitTests;

public class EnrollmentBusinessRulesTests
{
    private readonly FakeEnrollmentRepository _enrollmentRepo = new();
    private readonly FakeWorkshopRepository _workshopRepo = new();
    private readonly FakeParticipantRepository _participantRepo = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly EnrollmentService _service;

    public EnrollmentBusinessRulesTests()
    {
        _service = new EnrollmentService(_enrollmentRepo, _workshopRepo, _participantRepo, _uow);
    }

    [Fact]
    public async Task Enroll_WhenCapacityReached_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var participant = Participant.Create("Carlos Ruiz", "carlos@correo.com", "user-1");
        await _participantRepo.AddAsync(participant);

        var workshop = Workshop.Create("FOT-001", "Photography", "Basics", Guid.NewGuid(),
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(5).AddHours(2), 1);
        await _workshopRepo.AddAsync(workshop);

        // Fill capacity with another participant
        var existingEnrollment = Enrollment.Create(Guid.NewGuid(), workshop.Id);
        await _enrollmentRepo.AddAsync(existingEnrollment);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _service.EnrollAsync("user-1", new CreateEnrollmentDto(workshop.Id)));

        Assert.Contains("full", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Enroll_WhenAlreadyHasActiveEnrollment_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var participant = Participant.Create("Carlos Ruiz", "carlos@correo.com", "user-2");
        await _participantRepo.AddAsync(participant);

        var workshop = Workshop.Create("FOT-002", "Photography", "Basics", Guid.NewGuid(),
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(5).AddHours(2), 10);
        await _workshopRepo.AddAsync(workshop);

        // Active enrollment already exists
        var existingEnrollment = Enrollment.Create(participant.Id, workshop.Id);
        await _enrollmentRepo.AddAsync(existingEnrollment);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _service.EnrollAsync("user-2", new CreateEnrollmentDto(workshop.Id)));

        Assert.Contains("already have an active enrollment", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Enroll_WhenSchedulesOverlap_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange: Participant has an active workshop on Day 5 from 10:00 to 14:00
        var participant = Participant.Create("Carlos Ruiz", "carlos@correo.com", "user-3");
        await _participantRepo.AddAsync(participant);

        var baseTime = DateTime.UtcNow.Date.AddDays(5);
        var workshop1 = Workshop.Create("COC-001", "Cooking 101", "Costeña", Guid.NewGuid(),
            baseTime.AddHours(10), baseTime.AddHours(14), 10);
        await _workshopRepo.AddAsync(workshop1);

        var enrollment1 = Enrollment.Create(participant.Id, workshop1.Id);
        await _enrollmentRepo.AddAsync(enrollment1);

        // Workshop 2 overlaps: Day 5 from 12:00 to 16:00
        var workshop2 = Workshop.Create("ORA-001", "Oratoria", "Speech", Guid.NewGuid(),
            baseTime.AddHours(12), baseTime.AddHours(16), 10);
        await _workshopRepo.AddAsync(workshop2);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _service.EnrollAsync("user-3", new CreateEnrollmentDto(workshop2.Id)));

        Assert.Contains("overlap", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Enroll_WhenPreviousEnrollmentWasCancelled_ShouldAllowReEnrollment()
    {
        // Arrange
        var participant = Participant.Create("Carlos Ruiz", "carlos@correo.com", "user-4");
        await _participantRepo.AddAsync(participant);

        var workshop = Workshop.Create("DIS-001", "Design", "Intro", Guid.NewGuid(),
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(5).AddHours(2), 10);
        await _workshopRepo.AddAsync(workshop);

        // Previously cancelled enrollment
        var previousEnrollment = Enrollment.Create(participant.Id, workshop.Id);
        previousEnrollment.Cancel("user-4");
        await _enrollmentRepo.AddAsync(previousEnrollment);

        // Act
        var result = await _service.EnrollAsync("user-4", new CreateEnrollmentDto(workshop.Id));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Active", result.Status);
        Assert.Equal(workshop.Id, result.WorkshopId);
    }

    [Fact]
    public async Task Cancel_ByParticipant_Within24HoursOfStart_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange: Workshop starts in 10 hours (less than 24 hours)
        var participant = Participant.Create("Carlos Ruiz", "carlos@correo.com", "user-5");
        await _participantRepo.AddAsync(participant);

        var workshop = Workshop.Create("FOT-005", "Photo", "Desc", Guid.NewGuid(),
            DateTime.UtcNow.AddHours(10), DateTime.UtcNow.AddHours(12), 10);
        await _workshopRepo.AddAsync(workshop);

        var enrollment = Enrollment.Create(participant.Id, workshop.Id);
        await _enrollmentRepo.AddAsync(enrollment);

        // Act & Assert: Participant attempts to cancel
        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _service.CancelEnrollmentAsync(enrollment.Id, "user-5", isEmployee: false));

        Assert.Contains("24 hours", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Cancel_ByEmployee_Within24HoursOfStart_ShouldSucceed()
    {
        // Arrange: Workshop starts in 5 hours
        var participant = Participant.Create("Carlos Ruiz", "carlos@correo.com", "user-6");
        await _participantRepo.AddAsync(participant);

        var workshop = Workshop.Create("FOT-006", "Photo", "Desc", Guid.NewGuid(),
            DateTime.UtcNow.AddHours(5), DateTime.UtcNow.AddHours(7), 10);
        await _workshopRepo.AddAsync(workshop);

        var enrollment = Enrollment.Create(participant.Id, workshop.Id);
        await _enrollmentRepo.AddAsync(enrollment);

        // Act: Employee cancels
        var result = await _service.CancelEnrollmentAsync(enrollment.Id, "employee@correo.com", isEmployee: true);

        // Assert
        Assert.Equal("Cancelled", result.Status);
        Assert.NotNull(result.CancelledAt);
        Assert.Equal("employee@correo.com", result.CancelledBy);
    }

    [Fact]
    public async Task Cancel_ByParticipant_MoreThan24HoursBeforeStart_ShouldSucceed()
    {
        // Arrange: Workshop starts in 3 days
        var participant = Participant.Create("Carlos Ruiz", "carlos@correo.com", "user-7");
        await _participantRepo.AddAsync(participant);

        var workshop = Workshop.Create("FOT-007", "Photo", "Desc", Guid.NewGuid(),
            DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(3).AddHours(2), 10);
        await _workshopRepo.AddAsync(workshop);

        var enrollment = Enrollment.Create(participant.Id, workshop.Id);
        await _enrollmentRepo.AddAsync(enrollment);

        // Act: Participant cancels
        var result = await _service.CancelEnrollmentAsync(enrollment.Id, "user-7", isEmployee: false);

        // Assert
        Assert.Equal("Cancelled", result.Status);
        Assert.NotNull(result.CancelledAt);
        Assert.Equal("user-7", result.CancelledBy);
    }

    // --- In-Memory Test Doubles ---
    private class FakeEnrollmentRepository : IEnrollmentRepository
    {
        private readonly List<Enrollment> _enrollments = new();

        public Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_enrollments.FirstOrDefault(e => e.Id == id));

        public Task<IReadOnlyList<Enrollment>> GetByWorkshopIdAsync(Guid workshopId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Enrollment>>(_enrollments.Where(e => e.WorkshopId == workshopId).ToList());

        public Task<IReadOnlyList<Enrollment>> GetByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Enrollment>>(_enrollments.Where(e => e.ParticipantId == participantId).ToList());

        public Task<IReadOnlyList<Enrollment>> GetActiveByParticipantIdAsync(Guid participantId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Enrollment>>(_enrollments.Where(e => e.ParticipantId == participantId && e.Status == EnrollmentStatus.Active).ToList());

        public Task<Enrollment?> GetActiveEnrollmentAsync(Guid participantId, Guid workshopId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_enrollments.FirstOrDefault(e => e.ParticipantId == participantId && e.WorkshopId == workshopId && e.Status == EnrollmentStatus.Active));

        public Task<int> GetActiveEnrollmentCountAsync(Guid workshopId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_enrollments.Count(e => e.WorkshopId == workshopId && e.Status == EnrollmentStatus.Active));

        public Task<bool> HasActiveEnrollmentAsync(Guid participantId, Guid workshopId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_enrollments.Any(e => e.ParticipantId == participantId && e.WorkshopId == workshopId && e.Status == EnrollmentStatus.Active));

        public Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
        {
            _enrollments.Add(enrollment);
            return Task.CompletedTask;
        }

        public void Update(Enrollment enrollment) { }
        public void Delete(Enrollment enrollment) => _enrollments.Remove(enrollment);
    }

    private class FakeWorkshopRepository : IWorkshopRepository
    {
        private readonly List<Workshop> _workshops = new();

        public Task<Workshop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_workshops.FirstOrDefault(w => w.Id == id));

        public Task<Workshop?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_workshops.FirstOrDefault(w => w.Id == id));

        public Task<Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(_workshops.FirstOrDefault(w => w.Code == code.ToUpperInvariant()));

        public Task<IReadOnlyList<Workshop>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Workshop>>(_workshops.ToList());

        public Task<IReadOnlyList<Workshop>> GetUpcomingAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Workshop>>(_workshops.Where(w => w.StartDateTime > DateTime.UtcNow).ToList());

        public Task<bool> ExistsWithCodeAsync(string code, Guid? excludeWorkshopId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(_workshops.Any(w => w.Code == code.ToUpperInvariant() && (!excludeWorkshopId.HasValue || w.Id != excludeWorkshopId.Value)));

        public Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default)
        {
            _workshops.Add(workshop);
            return Task.CompletedTask;
        }

        public void Update(Workshop workshop) { }
        public void Delete(Workshop workshop) => _workshops.Remove(workshop);
    }

    private class FakeParticipantRepository : IParticipantRepository
    {
        private readonly List<Participant> _participants = new();

        public Task<Participant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_participants.FirstOrDefault(p => p.Id == id));

        public Task<Participant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_participants.FirstOrDefault(p => p.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

        public Task<Participant?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_participants.FirstOrDefault(p => p.UserId == userId));

        public Task<IReadOnlyList<Participant>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Participant>>(_participants.ToList());

        public Task AddAsync(Participant participant, CancellationToken cancellationToken = default)
        {
            _participants.Add(participant);
            return Task.CompletedTask;
        }

        public void Update(Participant participant) { }
        public void Delete(Participant participant) => _participants.Remove(participant);
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }
}
