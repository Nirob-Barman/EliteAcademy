using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Infrastructure.Identity.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> builder)
        {
            // Each student can wishlist a class only once
            builder.HasIndex(w => new { w.StudentId, w.ClassId }).IsUnique();

            builder.HasOne(w => w.Class)
                   .WithMany()
                   .HasForeignKey(w => w.ClassId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ApplicationIdentityUser>()
                   .WithMany()
                   .HasForeignKey(w => w.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
