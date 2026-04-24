namespace EliteAcademy.Application.DTOs.QA
{
    public class QaQuestionDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public DateTime AskedAt { get; set; }
        public List<QaAnswerDto> Answers { get; set; } = new();
    }

    public class QaAnswerDto
    {
        public int Id { get; set; }
        public string? InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public string AnswerText { get; set; } = string.Empty;
        public DateTime AnsweredAt { get; set; }
    }

    public class QaAskDto
    {
        public int ClassId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
    }

    public class QaAnswerFormDto
    {
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;
    }
}
