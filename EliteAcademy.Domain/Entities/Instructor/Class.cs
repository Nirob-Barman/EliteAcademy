using EliteAcademy.Domain.Entities.Student;
using EliteAcademy.Domain.Enums;

namespace EliteAcademy.Domain.Entities.Instructor
{
    public class Class : BaseEntity
    {
        public string? ClassName { get; set; }
        public string? ClassImage { get; set; }
        public string? InstructorId { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Price { get; set; }
        public ClassStatus Status { get; set; } = ClassStatus.Pending;
        public string? Feedback { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<PreEnrollment> PreEnrollments { get; set; } = new List<PreEnrollment>();
    }
}
