namespace EliteAcademy.Application.DTOs.Student
{
    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? ClassImage { get; set; }
        public string? InstructorName { get; set; }
        public decimal Price { get; set; }
        public DateTime EnrolledAt { get; set; }
    }
}
