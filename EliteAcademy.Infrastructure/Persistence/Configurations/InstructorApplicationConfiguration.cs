using EliteAcademy.Domain.Entities;
using EliteAcademy.Infrastructure.Identity.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class InstructorApplicationConfiguration : IEntityTypeConfiguration<InstructorApplication>
    {
        public void Configure(EntityTypeBuilder<InstructorApplication> builder)
        {
            builder.Property(a => a.FullName)  .HasMaxLength(200);
            builder.Property(a => a.Email)     .HasMaxLength(256);
            builder.Property(a => a.Expertise) .HasMaxLength(300);
            builder.Property(a => a.Bio)       .HasMaxLength(2000);
            builder.Property(a => a.Motivation).HasMaxLength(2000);
            builder.Property(a => a.AdminNotes).HasMaxLength(1000);

            builder.HasOne<ApplicationIdentityUser>()
                   .WithMany()
                   .HasForeignKey(a => a.ApplicantId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
