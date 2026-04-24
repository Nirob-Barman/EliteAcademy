using EliteAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class AppNotificationConfiguration : IEntityTypeConfiguration<AppNotification>
    {
        public void Configure(EntityTypeBuilder<AppNotification> builder)
        {
            builder.Property(n => n.Title).HasMaxLength(200);
            builder.Property(n => n.Message).HasMaxLength(1000);
            builder.Property(n => n.Link).HasMaxLength(500);
        }
    }
}
