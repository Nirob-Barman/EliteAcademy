using EliteAcademy.Application.DTOs.Student;

namespace EliteAcademy.Web.ViewModels.Student
{
    public class EnrolledClassesViewModel
    {
        public List<EnrollmentDto> Enrollments { get; set; } = new();
        public HashSet<int> ReviewedClassIds { get; set; } = new();
    }
}
