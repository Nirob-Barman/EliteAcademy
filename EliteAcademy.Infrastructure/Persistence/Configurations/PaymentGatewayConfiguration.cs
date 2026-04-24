using EliteAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class PaymentGatewayConfiguration : IEntityTypeConfiguration<PaymentGateway>
    {
        public void Configure(EntityTypeBuilder<PaymentGateway> builder)
        {
            builder.Property(g => g.Slug).HasMaxLength(50);
            builder.Property(g => g.Name).HasMaxLength(100);
            builder.HasIndex(g => g.Slug).IsUnique();
        }
    }
}
