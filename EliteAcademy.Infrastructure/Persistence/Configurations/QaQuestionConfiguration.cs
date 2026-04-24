using EliteAcademy.Domain.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EliteAcademy.Infrastructure.Persistence.Configurations
{
    public class QaQuestionConfiguration : IEntityTypeConfiguration<QaQuestion>
    {
        public void Configure(EntityTypeBuilder<QaQuestion> builder)
        {
            builder.HasOne(q => q.Class)
                   .WithMany()
                   .HasForeignKey(q => q.ClassId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(q => q.QuestionText).HasMaxLength(2000);
        }
    }
}
