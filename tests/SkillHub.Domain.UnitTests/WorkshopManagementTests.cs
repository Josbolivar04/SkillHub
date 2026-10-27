using SkillHub.Application.DTOs.Workshops;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.Repositories;
using SkillHub.Infrastructure.Services;
using Xunit;

namespace SkillHub.Domain.UnitTests;

public class WorkshopManagementTests
{
    private readonly FakeWorkshopRepository _workshopRepo = new();
    private readonly FakeInstructorRepository _instructorRepo = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly WorkshopService _service;

    public WorkshopManagementTests()
    {
        _service = new WorkshopService(_workshopRepo, _instructorRepo, _uow);
    }

    [Fact]
    public async Task CreateWorkshop_WhenInstructorDoesNotExist_ShouldThrowBusinessRuleViolationException()
    {
        var dto = new CreateWorkshopDto(
            "NEW-001", "New Title", "Description",
            "nonexistent@correo.com",
            DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(5).AddHours(3),
            20);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _service.CreateWorkshopAsync(dto));

        Assert.Contains("does not exist", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateWorkshop_WhenStartDateIsInThePast_ShouldThrowBusinessRuleViolationException()
    {
        var instructor = Instructor.Create("John Doe", "john@correo.com", "Coding");
        await _instructorRepo.AddAsync(instructor);

        var dto = new CreateWorkshopDto(
            "NEW-002", "New Title", "Description",
            "john@correo.com",
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddHours(2),
            20);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _service.CreateWorkshopAsync(dto));

        Assert.Contains("future", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateWorkshop_WhenEndDateIsBeforeStartDate_ShouldThrowBusinessRuleViolationException()
    {
        var instructor = Instructor.Create("John Doe", "john@correo.com", "Coding");
        await _instructorRepo.AddAsync(instructor);

        var start = DateTime.UtcNow.AddDays(3);
        var end = start.AddHours(-1);

        var dto = new CreateWorkshopDto(
            "NEW-003", "New Title", "Description",
            "john@correo.com",
            start, end,
            20);

        var ex = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            _service.CreateWorkshopAsync(dto));

        Assert.Contains("after", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateWorkshop_WithValidData_ShouldSucceedAndCalculateAvailableSlots()
    {
        var instructor = Instructor.Create("John Doe", "john@correo.com", "Coding");
        await _instructorRepo.AddAsync(instructor);

        var start = DateTime.UtcNow.AddDays(3);
        var end = start.AddHours(2);

        var dto = new CreateWorkshopDto(
            "NEW-004", "Valid Workshop", "Description",
            "john@correo.com",
            start, end,
            25);

        var result = await _service.CreateWorkshopAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("NEW-004", result.Code);
        Assert.Equal(25, result.Capacity);
        Assert.Equal(25, result.AvailableSlots);
        Assert.Equal(0, result.ActiveEnrollments);
        Assert.True(_uow.Saved);
    }

    [Fact]
    public async Task GetFutureWorkshops_ShouldReturnCalculatedAvailableSlots()
    {
        var instructor = Instructor.Create("John Doe", "john@correo.com", "Coding");
        var workshop = Workshop.Create("FOT-001", "Photo", "Desc", instructor.Id,
            DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(2), 10);

        await _workshopRepo.AddAsync(workshop);

        var result = await _service.GetFutureWorkshopsAsync();

        Assert.Single(result);
        Assert.Equal(10, result[0].AvailableSlots);
    }

    // --- In-Memory Test Doubles ---
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

    private class FakeInstructorRepository : IInstructorRepository
    {
        private readonly List<Instructor> _instructors = new();

        public Task<Instructor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_instructors.FirstOrDefault(i => i.Id == id));

        public Task<Instructor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_instructors.FirstOrDefault(i => i.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

        public Task<Instructor?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_instructors.FirstOrDefault(i => i.UserId == userId));

        public Task<IReadOnlyList<Instructor>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Instructor>>(_instructors.ToList());

        public Task AddAsync(Instructor instructor, CancellationToken cancellationToken = default)
        {
            _instructors.Add(instructor);
            return Task.CompletedTask;
        }

        public void Update(Instructor instructor) { }
        public void Delete(Instructor instructor) => _instructors.Remove(instructor);
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public bool Saved { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Saved = true;
            return Task.FromResult(1);
        }
    }
}
