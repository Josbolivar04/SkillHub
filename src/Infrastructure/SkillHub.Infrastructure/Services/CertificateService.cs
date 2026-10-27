using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.Repositories;

namespace SkillHub.Infrastructure.Services;

public class CertificateService : ICertificateService, IPdfGenerator
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IWorkshopRepository _workshopRepository;
    private readonly IParticipantRepository _participantRepository;

    public CertificateService(
        IEnrollmentRepository enrollmentRepository,
        IWorkshopRepository workshopRepository,
        IParticipantRepository participantRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _workshopRepository = workshopRepository;
        _participantRepository = participantRepository;

        // Configure QuestPDF community license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateEnrollmentCertificateAsync(
        Guid enrollmentId,
        string userIdentifier,
        bool isEmployee,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment == null)
        {
            throw new BusinessRuleViolationException("Enrollment was not found.");
        }

        // Rule: Only active enrollments have certificates
        if (enrollment.Status != EnrollmentStatus.Active)
        {
            throw new BusinessRuleViolationException("Cannot generate certificate for a cancelled enrollment.");
        }

        // Rule: Participant can only download their own certificate; Employee can download any
        if (!isEmployee)
        {
            var participantUser = await _participantRepository.GetByUserIdAsync(userIdentifier, cancellationToken)
                ?? await _participantRepository.GetByEmailAsync(userIdentifier, cancellationToken);

            if (participantUser == null || enrollment.ParticipantId != participantUser.Id)
            {
                throw new UnauthorizedAccessException("You are not authorized to download this enrollment certificate.");
            }
        }

        var workshop = enrollment.Workshop ?? await _workshopRepository.GetByIdWithDetailsAsync(enrollment.WorkshopId, cancellationToken);
        var participant = enrollment.Participant ?? await _participantRepository.GetByIdAsync(enrollment.ParticipantId, cancellationToken);

        if (workshop == null || participant == null)
        {
            throw new BusinessRuleViolationException("Required enrollment details could not be loaded.");
        }

        var generationDate = DateTime.UtcNow;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("SKILLHUB").Bold().FontSize(26).FontColor(Colors.Blue.Darken2);
                            c.Item().Text("Centro de Formación y Talleres Especializados").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                        row.ConstantItem(120).AlignRight().Text("Oficial").Bold().FontColor(Colors.Green.Darken1);
                    });

                    col.Item().PaddingVertical(15).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().AlignCenter().Text("CONSTANCIA DE INSCRIPCIÓN").Bold().FontSize(20).FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(5).AlignCenter().Text("Certificado Oficial de Registro de Taller").FontSize(12).Italic().FontColor(Colors.Grey.Darken1);

                    col.Item().PaddingTop(25).Text("Por medio de la presente, SkillHub hace constar que el siguiente participante se encuentra registrado y activo en el taller descrito a continuación:")
                        .FontSize(11).LineHeight(1.5f);

                    col.Item().PaddingTop(20).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Padding(15).Column(c =>
                    {
                        c.Item().Text("DATOS DEL PARTICIPANTE").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        c.Item().PaddingTop(5).Text($"Nombre Completo: {participant.FullName}").Bold();
                        c.Item().Text($"Correo Electrónico: {participant.Email}");
                    });

                    col.Item().PaddingTop(15).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Padding(15).Column(c =>
                    {
                        c.Item().Text("DETALLES DEL TALLER").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        c.Item().PaddingTop(5).Text($"Código del Taller: {workshop.Code}").Bold();
                        c.Item().Text($"Título del Taller: {workshop.Title}").Bold();
                        c.Item().Text($"Instructor Asignado: {workshop.Instructor?.FullName ?? "Instructor Certificado"} ({workshop.Instructor?.Email})");
                        c.Item().Text($"Fecha y Hora de Inicio: {workshop.StartDateTime:dd/MM/yyyy HH:mm} UTC");
                        c.Item().Text($"Fecha y Hora de Finalización: {workshop.EndDateTime:dd/MM/yyyy HH:mm} UTC");
                        c.Item().Text($"Cupo Máximo del Taller: {workshop.Capacity} participantes");
                    });

                    col.Item().PaddingTop(15).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Padding(15).Column(c =>
                    {
                        c.Item().Text("METADATOS DE INSCRIPCIÓN").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        c.Item().PaddingTop(5).Text($"ID de Registro: {enrollment.Id}");
                        c.Item().Text($"Estado de la Inscripción: {enrollment.Status}");
                        c.Item().Text($"Fecha de Inscripción: {enrollment.EnrolledAt:dd/MM/yyyy HH:mm:ss} UTC");
                        c.Item().Text($"Fecha de Emisión del Documento: {generationDate:dd/MM/yyyy HH:mm:ss} UTC");
                    });

                    col.Item().PaddingTop(30).Text("Esta constancia certifica la reserva de cupo para el taller indicado. Presentar este documento al momento de ingresar a las instalaciones.")
                        .FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text("SkillHub — Sistema de Gestión Académica y Talleres").FontSize(8).FontColor(Colors.Grey.Medium);
                        row.RelativeItem().AlignRight().Text($"Generado el {generationDate:yyyy-MM-dd}").FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
