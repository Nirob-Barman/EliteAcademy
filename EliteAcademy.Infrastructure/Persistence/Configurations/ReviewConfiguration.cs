using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Infrastructure.Identity.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.Property(r => r.Comment).HasMaxLength(1000);

            // One review per student per class
            builder.HasIndex(r => new { r.StudentId, r.ClassId }).IsUnique();

            builder.HasOne(r => r.Class)
                   .WithMany()
                   .HasForeignKey(r => r.ClassId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ApplicationIdentityUser>()
                   .WithMany()
                   .HasForeignKey(r => r.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
