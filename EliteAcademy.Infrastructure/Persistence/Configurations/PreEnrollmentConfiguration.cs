using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Infrastructure.Identity.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class PreEnrollmentConfiguration : IEntityTypeConfiguration<PreEnrollment>
    {
        public void Configure(EntityTypeBuilder<PreEnrollment> builder)
        {
            builder.Property(p => p.DiscountAmount).HasPrecision(18, 2);
            builder.Property(p => p.CouponCode).HasMaxLength(50);

            builder.HasOne<ApplicationIdentityUser>()
                   .WithMany()
                   .HasForeignKey(p => p.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
