using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.Domain.Entities;

namespace SkillHub.Infrastructure.Data.Configurations;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("Participants");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(p => p.Email);

        builder.Property(p => p.UserId)
            .HasMaxLength(450)
            .IsRequired(false);

        builder.Property(p => p.RegisteredAt)
            .IsRequired();

        builder.HasMany(p => p.Enrollments)
            .WithOne(e => e.Participant)
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(p => p.Enrollments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
