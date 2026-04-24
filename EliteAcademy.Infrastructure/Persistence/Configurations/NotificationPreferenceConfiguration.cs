using EliteAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
    {
        public void Configure(EntityTypeBuilder<NotificationPreference> builder)
        {
            builder.HasIndex(x => x.UserId).IsUnique();
            builder.Property(x => x.UserId).IsRequired().HasMaxLength(450);
        }
    }
}
