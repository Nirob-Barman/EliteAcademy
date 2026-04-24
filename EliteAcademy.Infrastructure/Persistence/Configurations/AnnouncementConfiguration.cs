using EliteAcademy.Domain.Entities.Instructor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.Property(a => a.Title).HasMaxLength(200);

            builder.HasOne(a => a.Class)
                   .WithMany()
                   .HasForeignKey(a => a.ClassId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
