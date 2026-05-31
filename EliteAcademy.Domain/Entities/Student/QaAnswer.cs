using EliteAcademy.Domain.Common;
using EliteAcademy.Domain.Entities.Instructor;

namespace EliteAcademy.Domain.Entities.Student
{
    public class QaAnswer : BaseEntity
    {
        public int QuestionId { get; set; }
        public QaQuestion? Question { get; set; }
        public string? InstructorId { get; set; }
        public string AnswerText { get; set; } = string.Empty;

        public static DomainResult<QaAnswer> Create(string instructorId, int questionId, string answerText, Class? cls)
        {
            if (string.IsNullOrWhiteSpace(answerText))
                return DomainResult<QaAnswer>.Fail("Answer cannot be empty.");
            if (cls == null || cls.InstructorId != instructorId)
                return DomainResult<QaAnswer>.Fail("Not authorized to answer this question.");

            return DomainResult<QaAnswer>.Ok(new QaAnswer
            {
                QuestionId = questionId,
                InstructorId = instructorId,
                AnswerText = answerText.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = instructorId
            });
        }
    }
}
