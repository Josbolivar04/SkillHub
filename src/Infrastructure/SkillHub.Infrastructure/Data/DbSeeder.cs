using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Data;

public class DbSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DbSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding process...");

        // Ensure database exists and schema is up to date
        await _context.Database.MigrateAsync();

        // 1. Seed Roles
        await SeedRolesAsync();

        // 2. Locate Excel file
        var excelPath = FindExcelDataFile();

        if (!string.IsNullOrEmpty(excelPath) && File.Exists(excelPath))
        {
            _logger.LogInformation("Seeding database using Excel file at: {Path}", excelPath);
            await SeedFromExcelAsync(excelPath);
        }
        else
        {
            _logger.LogWarning("SkillHub_Data.xlsx not found. Executing fallback code-based seeding.");
            await SeedFallbackDataAsync();
        }

        // 3. Ensure the specific example instructors from requirements exist
        await EnsurePromptExampleInstructorsAsync();

        _logger.LogInformation("Database seeding completed successfully.");
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                _logger.LogInformation("Created Identity role: {Role}", role);
            }
        }
    }

    private string? FindExcelDataFile()
    {
        var possiblePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Data", "SkillHub_Data.xlsx"),
            Path.Combine(AppContext.BaseDirectory, "SkillHub_Data.xlsx"),
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "SkillHub_Data.xlsx"),
            Path.Combine(Directory.GetCurrentDirectory(), "SkillHub_Data.xlsx"),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "Infrastructure", "SkillHub.Infrastructure", "Data", "SkillHub_Data.xlsx"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Infrastructure", "SkillHub.Infrastructure", "Data", "SkillHub_Data.xlsx"),
            @"C:\Users\boliv\Downloads\SkillHub_Data.xlsx"
        };

        return possiblePaths.FirstOrDefault(File.Exists);
    }

    private async Task SeedFromExcelAsync(string filePath)
    {
        ExcelPackage.License.SetNonCommercialPersonal("SkillHub");
        using var package = new ExcelPackage(new FileInfo(filePath));

        // Seed Instructors
        var instructorsSheet = package.Workbook.Worksheets["Instructors"] ?? package.Workbook.Worksheets[0];
        if (instructorsSheet != null && instructorsSheet.Dimension != null)
        {
            for (int r = 2; r <= instructorsSheet.Dimension.Rows; r++)
            {
                var fullName = instructorsSheet.Cells[r, 1].Text.Trim();
                var email = instructorsSheet.Cells[r, 2].Text.Trim().ToLowerInvariant();
                var password = instructorsSheet.Cells[r, 3].Text.Trim();
                var specialty = instructorsSheet.Cells[r, 4].Text.Trim();

                if (string.IsNullOrWhiteSpace(email)) continue;
                if (string.IsNullOrWhiteSpace(password)) password = "Instructor2026*";

                await CreateInstructorIfNotExistAsync(fullName, email, password, specialty);
            }
        }

        // Seed Participants
        var participantsSheet = package.Workbook.Worksheets["Participants"];
        if (participantsSheet != null && participantsSheet.Dimension != null)
        {
            for (int r = 2; r <= participantsSheet.Dimension.Rows; r++)
            {
                var fullName = participantsSheet.Cells[r, 1].Text.Trim();
                var email = participantsSheet.Cells[r, 2].Text.Trim().ToLowerInvariant();
                var password = participantsSheet.Cells[r, 3].Text.Trim();

                if (string.IsNullOrWhiteSpace(email)) continue;
                if (string.IsNullOrWhiteSpace(password)) password = "Skillhub2026*";

                await CreateParticipantIfNotExistAsync(fullName, email, password);
            }
        }

        await _context.SaveChangesAsync();

        // Seed Workshops
        var workshopsSheet = package.Workbook.Worksheets["Workshops"];
        if (workshopsSheet != null && workshopsSheet.Dimension != null)
        {
            for (int r = 2; r <= workshopsSheet.Dimension.Rows; r++)
            {
                var code = workshopsSheet.Cells[r, 1].Text.Trim().ToUpperInvariant();
                var title = workshopsSheet.Cells[r, 2].Text.Trim();
                var description = workshopsSheet.Cells[r, 3].Text.Trim();
                var instructorEmail = workshopsSheet.Cells[r, 4].Text.Trim().ToLowerInvariant();
                var startText = workshopsSheet.Cells[r, 5].Text.Trim();
                var endText = workshopsSheet.Cells[r, 6].Text.Trim();
                var capacityText = workshopsSheet.Cells[r, 7].Text.Trim();

                if (string.IsNullOrWhiteSpace(code)) continue;

                var existingWorkshop = await _context.Workshops.FirstOrDefaultAsync(w => w.Code == code);
                if (existingWorkshop != null) continue;

                var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.Email == instructorEmail);
                if (instructor == null)
                {
                    _logger.LogWarning("Instructor with email '{Email}' not found for workshop {Code}", instructorEmail, code);
                    continue;
                }

                if (!DateTime.TryParse(startText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDateTime))
                {
                    startDateTime = DateTime.UtcNow.AddDays(10 + r);
                }

                if (!DateTime.TryParse(endText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDateTime))
                {
                    endDateTime = startDateTime.AddHours(3);
                }

                // Invariant: Ensure start time is in the future
                if (startDateTime <= DateTime.UtcNow)
                {
                    startDateTime = DateTime.UtcNow.AddDays(7);
                    endDateTime = startDateTime.AddHours(3);
                }

                // Invariant: End time after start time
                if (endDateTime <= startDateTime)
                {
                    endDateTime = startDateTime.AddHours(3);
                }

                int capacity = int.TryParse(capacityText, out var cap) && cap > 0 ? cap : 15;

                var workshop = Workshop.Create(code, title, description, instructor.Id, startDateTime, endDateTime, capacity);
                await _context.Workshops.AddAsync(workshop);
                _logger.LogInformation("Seeded Workshop: {Code} - {Title}", code, title);
            }

            await _context.SaveChangesAsync();
        }

        // Seed Enrollments
        var enrollmentsSheet = package.Workbook.Worksheets["Enrollments"];
        if (enrollmentsSheet != null && enrollmentsSheet.Dimension != null)
        {
            for (int r = 2; r <= enrollmentsSheet.Dimension.Rows; r++)
            {
                var participantEmail = enrollmentsSheet.Cells[r, 1].Text.Trim().ToLowerInvariant();
                var workshopCode = enrollmentsSheet.Cells[r, 2].Text.Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(participantEmail) || string.IsNullOrWhiteSpace(workshopCode))
                    continue;

                var participant = await _context.Participants.FirstOrDefaultAsync(p => p.Email == participantEmail);
                var workshop = await _context.Workshops.FirstOrDefaultAsync(w => w.Code == workshopCode);

                if (participant != null && workshop != null)
                {
                    var exists = await _context.Enrollments.AnyAsync(e =>
                        e.ParticipantId == participant.Id && e.WorkshopId == workshop.Id);

                    if (!exists)
                    {
                        var enrollment = Enrollment.Create(participant.Id, workshop.Id);
                        await _context.Enrollments.AddAsync(enrollment);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }

    private async Task EnsurePromptExampleInstructorsAsync()
    {
        var exampleInstructors = new[]
        {
            ("Andrés Mejía", "andres.mejia@correo.com", "Instructor2026*", "Fotografía"),
            ("Camila Torres", "camila.torres@correo.com", "Instructor2026*", "Cocina"),
            ("Lucía Gómez", "lucia.gomez@correo.com", "Instructor2026*", "Oratoria"),
            ("Mateo Morales", "mateo.morales@correo.com", "Instructor2026*", "Diseño"),
            ("Sofía Vargas", "sofia.vargas@correo.com", "Instructor2026*", "Fotografía"),
            ("Valentina Ríos", "valentina.rios@correo.com", "Instructor2026*", "Cocina")
        };

        foreach (var (fullName, email, password, specialty) in exampleInstructors)
        {
            await CreateInstructorIfNotExistAsync(fullName, email, password, specialty);
        }

        await _context.SaveChangesAsync();
    }

    private async Task CreateInstructorIfNotExistAsync(string fullName, string email, string password, string specialty)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                FullName = fullName.Trim(),
                Specialty = specialty.Trim(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Roles.Employee);
                _logger.LogInformation("Created Employee User: {Email}", normalizedEmail);
            }
            else
            {
                _logger.LogWarning("Failed to create instructor user '{Email}': {Errors}",
                    normalizedEmail, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            if (!await _userManager.IsInRoleAsync(user, Roles.Employee))
            {
                await _userManager.AddToRoleAsync(user, Roles.Employee);
            }
        }

        var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.Email == normalizedEmail);
        if (instructor == null)
        {
            instructor = new Instructor(Guid.NewGuid(), fullName.Trim(), normalizedEmail, specialty.Trim(), user?.Id);
            await _context.Instructors.AddAsync(instructor);
            _logger.LogInformation("Created Instructor entity: {FullName} ({Email})", fullName, normalizedEmail);
        }
    }

    private async Task CreateParticipantIfNotExistAsync(string fullName, string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                FullName = fullName.Trim(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Roles.Participant);
                _logger.LogInformation("Created Participant User: {Email}", normalizedEmail);
            }
            else
            {
                _logger.LogWarning("Failed to create participant user '{Email}': {Errors}",
                    normalizedEmail, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            if (!await _userManager.IsInRoleAsync(user, Roles.Participant))
            {
                await _userManager.AddToRoleAsync(user, Roles.Participant);
            }
        }

        var participant = await _context.Participants.FirstOrDefaultAsync(p => p.Email == normalizedEmail);
        if (participant == null)
        {
            participant = new Participant(Guid.NewGuid(), fullName.Trim(), normalizedEmail, user?.Id, DateTime.UtcNow);
            await _context.Participants.AddAsync(participant);
        }
    }

    private async Task SeedFallbackDataAsync()
    {
        await EnsurePromptExampleInstructorsAsync();

        // Seed 30 default participants
        for (int i = 1; i <= 30; i++)
        {
            var email = $"participante{i}@correo.com";
            var name = $"Participante {i}";
            await CreateParticipantIfNotExistAsync(name, email, "Skillhub2026*");
        }

        await _context.SaveChangesAsync();

        // Seed 15 default workshops
        var firstInstructor = await _context.Instructors.FirstOrDefaultAsync();
        if (firstInstructor != null)
        {
            for (int i = 1; i <= 15; i++)
            {
                var code = $"SKL-{i:D3}";
                if (!await _context.Workshops.AnyAsync(w => w.Code == code))
                {
                    var start = DateTime.UtcNow.AddDays(7 + i);
                    var end = start.AddHours(3);
                    var workshop = Workshop.Create(code, $"Workshop {i}", $"Description for workshop {i}", firstInstructor.Id, start, end, 15);
                    await _context.Workshops.AddAsync(workshop);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
