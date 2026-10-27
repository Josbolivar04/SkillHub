using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.Domain.Entities;

namespace SkillHub.Infrastructure.Data.Configurations;

public class WorkshopConfiguration : IEntityTypeConfiguration<Workshop>
{
    public void Configure(EntityTypeBuilder<Workshop> builder)
    {
        builder.ToTable("Workshops");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(w => w.Code)
            .IsUnique();

        builder.Property(w => w.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(2000);

        builder.Property(w => w.Capacity)
            .IsRequired();

        builder.Property(w => w.StartDateTime)
            .IsRequired();

        builder.Property(w => w.EndDateTime)
            .IsRequired();

        builder.HasOne(w => w.Instructor)
            .WithMany(i => i.Workshops)
            .HasForeignKey(w => w.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Enrollments)
            .WithOne(e => e.Workshop)
            .HasForeignKey(e => e.WorkshopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Enrollments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
