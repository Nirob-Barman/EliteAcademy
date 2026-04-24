using EliteAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.Property(a => a.EntityName).HasMaxLength(100);
            builder.Property(a => a.Action).HasMaxLength(100);
            builder.Property(a => a.UserName).HasMaxLength(256);
        }
    }
}
