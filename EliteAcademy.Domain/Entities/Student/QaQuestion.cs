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
    }
}
