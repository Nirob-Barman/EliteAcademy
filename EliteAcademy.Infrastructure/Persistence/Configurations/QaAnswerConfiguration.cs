using EliteAcademy.Domain.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class QaAnswerConfiguration : IEntityTypeConfiguration<QaAnswer>
    {
        public void Configure(EntityTypeBuilder<QaAnswer> builder)
        {
            builder.HasOne(a => a.Question)
                   .WithMany(q => q.Answers)
                   .HasForeignKey(a => a.QuestionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(a => a.AnswerText).HasMaxLength(4000);
        }
    }
}
