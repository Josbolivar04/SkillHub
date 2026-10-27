using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.Domain.Entities;

namespace SkillHub.Infrastructure.Data.Configurations;

public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.ToTable("Instructors");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(i => i.Email);

        builder.Property(i => i.Specialty)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.UserId)
            .HasMaxLength(450)
            .IsRequired(false);

        builder.HasMany(i => i.Workshops)
            .WithOne(w => w.Instructor)
            .HasForeignKey(w => w.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(i => i.Workshops)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
