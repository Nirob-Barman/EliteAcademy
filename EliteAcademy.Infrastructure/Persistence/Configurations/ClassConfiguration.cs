using EliteAcademy.Domain.Entities.Instructor;
using EliteAcademy.Infrastructure.Identity.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.Property(c => c.Price).HasPrecision(18, 2);
            builder.Property(c => c.ClassName).HasMaxLength(200);

            builder.HasOne<ApplicationIdentityUser>()
                   .WithMany()
                   .HasForeignKey(c => c.InstructorId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(c => c.Enrollments)
                   .WithOne(e => e.Class)
                   .HasForeignKey(e => e.ClassId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.PreEnrollments)
                   .WithOne(p => p.Class)
                   .HasForeignKey(p => p.ClassId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
