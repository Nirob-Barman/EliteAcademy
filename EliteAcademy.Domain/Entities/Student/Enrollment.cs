using EliteAcademy.Domain.Entities.Instructor;

namespace EliteAcademy.Domain.Entities.Student
{
    public class Enrollment : BaseEntity
    {
        public int ClassId { get; set; }
        public Class? Class { get; set; }
        public string? StudentId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public static Enrollment Create(string studentId, int classId) => new()
        {
            ClassId = classId,
            StudentId = studentId,
            EnrolledAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = studentId
        };
    }
}
