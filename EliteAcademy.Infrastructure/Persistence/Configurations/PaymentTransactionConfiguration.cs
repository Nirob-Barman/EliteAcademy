using EliteAcademy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.Property(t => t.Amount).HasPrecision(18, 2);
            builder.Property(t => t.SessionRef).HasMaxLength(500);

            builder.HasOne(t => t.PreEnrollment)
                   .WithMany()
                   .HasForeignKey(t => t.PreEnrollmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Gateway)
                   .WithMany(g => g.Transactions)
                   .HasForeignKey(t => t.GatewayId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
