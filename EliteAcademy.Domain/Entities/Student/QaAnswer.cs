namespace EliteAcademy.Domain.Entities.Student
{
    public class QaAnswer : BaseEntity
    {
        public int QuestionId { get; set; }
        public QaQuestion? Question { get; set; }
        public string? InstructorId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
    }
}
