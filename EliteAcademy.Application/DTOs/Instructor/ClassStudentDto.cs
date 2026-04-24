namespace EliteAcademy.Application.DTOs.Instructor
{
    public class ClassStudentDto
    {
        public string? StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime EnrolledAt { get; set; }
    }
}
