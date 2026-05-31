using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Entities.Instructor;

namespace EliteAcademy.Domain.Entities.Student
{
    public class QaQuestion : BaseEntity
    {
        public int ClassId { get; set; }
        public Class? Class { get; set; }
        public string? StudentId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public ICollection<QaAnswer> Answers { get; set; } = new List<QaAnswer>();

        public static DomainResult<QaQuestion> Create(string studentId, int classId, string questionText)
        {
            if (string.IsNullOrWhiteSpace(questionText))
                return DomainResult<QaQuestion>.Fail("Question cannot be empty.");

            return DomainResult<QaQuestion>.Ok(new QaQuestion
            {
                ClassId = classId,
                StudentId = studentId,
                QuestionText = questionText.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = studentId
            });
        }
    }
}
